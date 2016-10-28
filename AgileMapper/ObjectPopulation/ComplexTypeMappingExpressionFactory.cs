namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly ComplexTypeConstructionFactory _constructionFactory;

        public ComplexTypeMappingExpressionFactory(MapperContext mapperContext)
        {
            _constructionFactory = new ComplexTypeConstructionFactory(mapperContext);
        }

        protected override bool TargetTypeIsNotConstructable(IObjectMappingData mappingData)
            => _constructionFactory.GetNewObjectCreation(mappingData) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
        {
            var strategyShortCircuitReturns = GetStrategyShortCircuitReturnsOrNull(returnNull, mapperData);
            if (strategyShortCircuitReturns != null)
            {
                yield return strategyShortCircuitReturns;
            }

            var alreadyMappedShortCircuit = GetAlreadyMappedObjectShortCircuitOrNull(returnNull.Target, mapperData);
            if (alreadyMappedShortCircuit != null)
            {
                yield return alreadyMappedShortCircuit;
            }
        }

        private static Expression GetStrategyShortCircuitReturnsOrNull(Expression returnNull, ObjectMapperData mapperData)
        {
            if (mapperData.IsPartOfDerivedTypeMapping || mapperData.HasSameSourceAsParent())
            {
                return null;
            }

            var shortCircuitConditions = mapperData
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(mapperData)
                .WhereNotNull()
                .Select(condition => (Expression)Expression.IfThen(condition, returnNull))
                .ToArray();

            if (shortCircuitConditions.None())
            {
                return null;
            }

            var shortCircuitBlock = Expression.Block(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static readonly MethodInfo _tryGetMethod = typeof(IObjectMappingData).GetMethod("TryGet");

        private static Expression GetAlreadyMappedObjectShortCircuitOrNull(LabelTarget returnTarget, ObjectMapperData mapperData)
        {
            if (mapperData.TargetTypeHasNotYetBeenMapped)
            {
                return null;
            }

            var tryGetCall = Expression.Call(
                mapperData.MappingDataObject,
                _tryGetMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.InstanceVariable);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(returnTarget, mapperData.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override Expression GetTypeTests(ObjectMapperData mapperData)
            => ComplexTypeTypeTestsFactory.CreateFor(mapperData);

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var preCreationCallback = GetCreationCallbackOrEmpty(CallbackPosition.Before, mapperData);
            var postCreationCallback = GetCreationCallbackOrEmpty(CallbackPosition.After, mapperData);

            yield return preCreationCallback;

            var instanceVariableValue = GetObjectResolution(mappingData, postCreationCallback != Constants.EmptyExpression);
            var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, instanceVariableValue);
            yield return instanceVariableAssignment;

            yield return postCreationCallback;

            var registrationCall = GetObjectRegistrationCallOrNull(mapperData);
            if (registrationCall != null)
            {
                yield return registrationCall;
            }

            foreach (var population in GetPopulationsAndCallbacks(mappingData))
            {
                yield return population;
            }
        }

        private static Expression GetCreationCallbackOrEmpty(CallbackPosition callbackPosition, IMemberMapperData mapperData)
            => GetCallbackOrEmpty(c => c.GetCreationCallbackOrNull(callbackPosition, mapperData), mapperData);

        private Expression GetObjectResolution(IObjectMappingData mappingData, bool postCreationCallbackExists)
        {
            var objectCreationValue = _constructionFactory.GetNewObjectCreation(mappingData);

            if (postCreationCallbackExists)
            {
                objectCreationValue = Expression.Assign(mappingData.MapperData.CreatedObject, objectCreationValue);
            }

            var instanceDataTargetAssignment = Expression.Assign(mappingData.MapperData.TargetObject, objectCreationValue);
            var existingOrCreatedObject = Expression.Coalesce(mappingData.MapperData.TargetObject, instanceDataTargetAssignment);

            return existingOrCreatedObject;
        }

        private static readonly MethodInfo _registerMethod = typeof(IObjectMappingData).GetMethod("Register");

        private static Expression GetObjectRegistrationCallOrNull(ObjectMapperData mapperData)
        {
            if (mapperData.TargetTypeWillNotBeMappedAgain)
            {
                return null;
            }

            return Expression.Call(
                mapperData.MappingDataObject,
                _registerMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                mapperData.SourceObject,
                mapperData.InstanceVariable);
        }

        private static IEnumerable<Expression> GetPopulationsAndCallbacks(IObjectMappingData mappingData)
        {
            var sourceMemberTypeTests = new List<Expression>();

            foreach (var memberPopulation in MemberPopulationFactory.Create(mappingData))
            {
                if (!memberPopulation.IsSuccessful)
                {
                    yield return memberPopulation.GetPopulation();
                    continue;
                }

                var prePopulationCallback = GetPopulationCallbackOrEmpty(CallbackPosition.Before, memberPopulation, mappingData);

                if (prePopulationCallback != Constants.EmptyExpression)
                {
                    yield return prePopulationCallback;
                }

                yield return memberPopulation.GetPopulation();

                var postPopulationCallback = GetPopulationCallbackOrEmpty(CallbackPosition.After, memberPopulation, mappingData);

                if (postPopulationCallback != Constants.EmptyExpression)
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

        private static Expression GetPopulationCallbackOrEmpty(
            CallbackPosition position,
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            return GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(position, memberPopulation.MapperData, mappingData.MapperData),
                mappingData.MapperData);
        }

        private static void CreateSourceMemberTypeTesterIfRequired(
            ICollection<Expression> typeTests,
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

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.InstanceVariable;

        public void Reset() => _constructionFactory.Reset();
    }
}