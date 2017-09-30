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

            if (TryGetShortCircuitFactory(mapperData, out var sourceShortCircuitFactory))
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
            if (!mapperData.MappedObjectCachingNeeded || mapperData.TargetTypeHasNotYetBeenMapped)
            {
                return null;
            }

            var tryGetMethod = typeof(IObjectMappingDataUntyped).GetMethod("TryGet");

            var tryGetCall = Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                tryGetMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.TargetInstance);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(mapperData.ReturnLabelTarget, mapperData.TargetInstance));

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

            yield return preCreationCallback;

            if (mapperData.Context.UseLocalVariable)
            {
                var assignCreatedObject = postCreationCallback != null;
                var hasMemberPopulations = MemberPopulationsExist(populationsAndCallbacks);

                yield return GetLocalVariableInstantiation(assignCreatedObject, hasMemberPopulations, mappingData);
            }

            yield return postCreationCallback;

            yield return GetObjectRegistrationCallOrNull(mapperData);

            foreach (var population in populationsAndCallbacks)
            {
                yield return population;
            }

            mappingData.MapperKey.AddSourceMemberTypeTesterIfRequired(mappingData);
        }

        private static Expression GetCreationCallbackOrNull(CallbackPosition callbackPosition, IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetCreationCallbackOrNull(callbackPosition, mapperData);

        #region Object Registration

        private static readonly MethodInfo _registerMethod = typeof(IObjectMappingDataUntyped).GetMethod("Register");

        private static Expression GetObjectRegistrationCallOrNull(ObjectMapperData mapperData)
        {
            if (!mapperData.MappedObjectCachingNeeded || mapperData.TargetTypeWillNotBeMappedAgain)
            {
                return null;
            }

            return Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                _registerMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.TargetInstance);
        }

        #endregion

        private static IEnumerable<Expression> GetPopulationsAndCallbacks(IObjectMappingData mappingData)
        {
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
            }
        }

        private static Expression GetPopulationCallbackOrNull(
            CallbackPosition position,
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            return GetMappingCallbackOrNull(position, memberPopulation.MapperData, mappingData.MapperData);
        }

        private Expression GetLocalVariableInstantiation(bool assignCreatedObject, bool hasMemberPopulations, IObjectMappingData mappingData)
        {
            var localVariableValue = TargetObjectResolutionFactory.GetObjectResolution(
                md => _constructionFactory.GetNewObjectCreation(md),
                mappingData,
                assignCreatedObject,
                hasMemberPopulations: hasMemberPopulations);

            var localVariableAssignment = mappingData.MapperData.LocalVariable.AssignTo(localVariableValue);

            return localVariableAssignment;
        }

        private static bool MemberPopulationsExist(IEnumerable<Expression> populationsAndCallbacks)
            => populationsAndCallbacks.Any(population => population.NodeType != ExpressionType.Constant);

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.TargetInstance;

        public override void Reset() => _constructionFactory.Reset();
    }
}