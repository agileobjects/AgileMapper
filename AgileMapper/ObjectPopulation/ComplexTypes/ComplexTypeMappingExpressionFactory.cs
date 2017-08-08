namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using Members.Population;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly ComplexTypeConstructionFactory _constructionFactory;
        private readonly IEnumerable<ISourceShortCircuitFactory> _shortCircuitFactories;

        public ComplexTypeMappingExpressionFactory(MapperContext mapperContext)
        {
            _constructionFactory = new ComplexTypeConstructionFactory(mapperContext);

            _shortCircuitFactories = new[]
            {
                SourceDictionaryShortCircuitFactory.Instance
            };
        }

        public override bool IsFor(IObjectMappingData mappingData) => true;

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperData.TargetCouldBePopulated())
            {
                // If a target complex type is readonly or unconstructable 
                // we still try to map to it using an existing non-null value:
                nullMappingBlock = null;
                return false;
            }

            if (_constructionFactory.GetNewObjectCreation(mappingData) != null)
            {
                nullMappingBlock = null;
                return false;
            }

            var targetType = mappingData.MapperData.TargetType;

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("Cannot construct an instance of " + targetType.GetFriendlyName()),
                targetType.ToDefaultExpression());

            return true;
        }

        #region Short-Circuits

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (SourceObjectCouldBeNull(mapperData))
            {
                yield return Expression.IfThen(mapperData.SourceObject.GetIsDefaultComparison(), returnNull);
            }

            var alreadyMappedShortCircuit = GetAlreadyMappedObjectShortCircuitOrNull(mapperData);
            if (alreadyMappedShortCircuit != null)
            {
                yield return alreadyMappedShortCircuit;
            }

            ISourceShortCircuitFactory sourceShortCircuitFactory;

            if (TryGetShortCircuitFactory(mapperData, out sourceShortCircuitFactory))
            {
                yield return sourceShortCircuitFactory.GetShortCircuit(mappingData);
            }
        }

        private static bool SourceObjectCouldBeNull(IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType)
            {
                return false;
            }

            if (mapperData.TargetMemberIsEnumerableElement())
            {
                return !mapperData.HasSameSourceAsParent();
            }

            return false;
        }

        private static Expression GetAlreadyMappedObjectShortCircuitOrNull(ObjectMapperData mapperData)
        {
            if (mapperData.TargetTypeHasNotYetBeenMapped)
            {
                return null;
            }

            if (mapperData.MapperContext.UserConfigurations.DisableObjectTracking(mapperData))
            {
                return null;
            }

            var tryGetMethod = typeof(IObjectMappingDataUntyped).GetMethod("TryGet");

            var tryGetCall = Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                tryGetMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.InstanceVariable);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(mapperData.ReturnLabelTarget, mapperData.InstanceVariable));

            return ifTryGetReturn;
        }

        private bool TryGetShortCircuitFactory(ObjectMapperData mapperData, out ISourceShortCircuitFactory applicableFactory)
        {
            applicableFactory = _shortCircuitFactories.FirstOrDefault(f => f.IsFor(mapperData));
            return applicableFactory != null;
        }

        #endregion

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => DerivedComplexTypeMappingsFactory.CreateFor(mappingData);

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var preCreationCallback = GetCreationCallbackOrNull(CallbackPosition.Before, mapperData);
            var postCreationCallback = GetCreationCallbackOrNull(CallbackPosition.After, mapperData);
            var populationsAndCallbacks = GetPopulationsAndCallbacks(mappingData).ToArray();

            if (preCreationCallback != null)
            {
                yield return preCreationCallback;
            }

            var assignCreatedObject = postCreationCallback != null;
            var hasMemberPopulations = MemberPopulationsExist(populationsAndCallbacks);

            var instanceVariableValue = TargetObjectResolutionFactory.GetObjectResolution(
                md => _constructionFactory.GetNewObjectCreation(md),
                mappingData,
                assignCreatedObject,
                hasMemberPopulations: hasMemberPopulations);

            var instanceVariableAssignment = mapperData.InstanceVariable.AssignTo(instanceVariableValue);
            yield return instanceVariableAssignment;

            if (postCreationCallback != null)
            {
                yield return postCreationCallback;
            }

            var registrationCall = GetObjectRegistrationCallOrNull(mapperData);
            if (registrationCall != null)
            {
                yield return registrationCall;
            }

            foreach (var population in populationsAndCallbacks)
            {
                yield return population;
            }
        }

        private static Expression GetCreationCallbackOrNull(CallbackPosition callbackPosition, IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetCreationCallbackOrNull(callbackPosition, mapperData);

        #region Object Registration

        private static readonly MethodInfo _registerMethod = typeof(IObjectMappingDataUntyped).GetMethod("Register");

        private static Expression GetObjectRegistrationCallOrNull(ObjectMapperData mapperData)
        {
            if (mapperData.TargetTypeWillNotBeMappedAgain)
            {
                return null;
            }

            if (mapperData.MapperContext.UserConfigurations.DisableObjectTracking(mapperData))
            {
                return null;
            }

            return Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                _registerMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.InstanceVariable);
        }

        #endregion

        private static IEnumerable<Expression> GetPopulationsAndCallbacks(IObjectMappingData mappingData)
        {
            var sourceMemberTypeTests = new List<Expression>();

            foreach (var memberPopulation in MemberPopulationFactory.Default.Create(mappingData))
            {
                if (!memberPopulation.IsSuccessful)
                {
                    yield return memberPopulation.GetPopulation();
                    continue;
                }

                var prePopulationCallback = GetPopulationCallbackOrNull(CallbackPosition.Before, memberPopulation, mappingData);

                if (prePopulationCallback != null)
                {
                    yield return prePopulationCallback;
                }

                yield return memberPopulation.GetPopulation();

                var postPopulationCallback = GetPopulationCallbackOrNull(CallbackPosition.After, memberPopulation, mappingData);

                if (postPopulationCallback != null)
                {
                    yield return postPopulationCallback;
                }

                if (memberPopulation.SourceMemberTypeTest != null)
                {
                    sourceMemberTypeTests.Add(memberPopulation.SourceMemberTypeTest);
                }
            }

            CreateSourceMemberTypeTesterIfRequired(sourceMemberTypeTests, mappingData);
        }

        private static Expression GetPopulationCallbackOrNull(
            CallbackPosition position,
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            return GetMappingCallbackOrNull(position, memberPopulation.MapperData, mappingData.MapperData);
        }

        private static void CreateSourceMemberTypeTesterIfRequired(
            IList<Expression> typeTests,
            IObjectMappingData mappingData)
        {
            if (typeTests.None())
            {
                return;
            }

            var typeTest = typeTests.AndTogether();
            var typeTestLambda = Expression.Lambda<Func<IMappingData, bool>>(typeTest, Parameters.MappingData);

            mappingData.MapperKey.AddSourceMemberTypeTester(typeTestLambda.Compile());
        }

        private static bool MemberPopulationsExist(IEnumerable<Expression> populationsAndCallbacks)
            => populationsAndCallbacks.Any(population => population.NodeType != ExpressionType.Constant);

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;

        public override void Reset() => _constructionFactory.Reset();
    }
}