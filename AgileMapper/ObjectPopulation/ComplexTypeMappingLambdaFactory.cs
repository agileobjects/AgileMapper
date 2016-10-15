namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        private readonly ComplexTypeConstructionFactory _constructionFactory;

        public ComplexTypeMappingLambdaFactory(MapperContext mapperContext)
        {
            _constructionFactory = new ComplexTypeConstructionFactory(mapperContext);
        }

        protected override bool IsNotConstructable(IObjectMappingData mappingData)
            => _constructionFactory.GetNewObjectCreation(mappingData) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(
            GotoExpression returnNull,
            ObjectMapperData mapperData)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, mapperData);
            yield return GetAlreadyMappedObjectShortCircuit(returnNull.Target, mapperData);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, MemberMapperData mapperData)
        {
            if (!mapperData.SourceMember.Matches(mapperData.TargetMember))
            {
                return Constants.EmptyExpression;
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
                return Constants.EmptyExpression;
            }

            var shortCircuitBlock = Expression.Block(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static readonly MethodInfo _tryGetMethod = typeof(IObjectMappingData).GetMethod("TryGet");

        private static Expression GetAlreadyMappedObjectShortCircuit(LabelTarget returnTarget, MemberMapperData mapperData)
        {
            var tryGetCall = Expression.Call(
                mapperData.Parameter,
                _tryGetMethod.MakeGenericMethod(mapperData.SourceType, mapperData.InstanceVariable.Type),
                mapperData.SourceObject,
                mapperData.InstanceVariable);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(returnTarget, mapperData.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            yield return GetCreationCallback(CallbackPosition.Before, mapperData);

            var instanceVariableValue = GetObjectResolution(mappingData);
            var instanceVariableAssignment = Expression.Assign(mapperData.InstanceVariable, instanceVariableValue);
            yield return instanceVariableAssignment;

            yield return GetCreationCallback(CallbackPosition.After, mapperData);

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

        private static Expression GetCreationCallback(CallbackPosition callbackPosition, MemberMapperData mapperData)
            => GetCallbackOrEmpty(c => c.GetCreationCallbackOrNull(callbackPosition, mapperData), mapperData);

        private Expression GetObjectResolution(IObjectMappingData mappingData)
        {
            var objectCreation = _constructionFactory.GetNewObjectCreation(mappingData);
            var createdObjectAssignment = Expression.Assign(mappingData.MapperData.CreatedObject, objectCreation);
            var instanceDataTargetAssignment = Expression.Assign(mappingData.MapperData.TargetObject, createdObjectAssignment);
            var existingOrCreatedObject = Expression.Coalesce(mappingData.MapperData.TargetObject, instanceDataTargetAssignment);

            return existingOrCreatedObject;
        }

        private static readonly MethodInfo _registerMethod = typeof(IObjectMappingData).GetMethod("Register");

        private static Expression GetObjectRegistrationCallOrNull(MemberMapperData mapperData)
        {
            if (IsEnumerableElementMapping(mapperData) || SourceAndTargetAreExactMatches(mapperData))
            {
                return Expression.Call(
                    mapperData.Parameter,
                    _registerMethod.MakeGenericMethod(mapperData.SourceType, mapperData.TargetType),
                    mapperData.SourceObject,
                    mapperData.InstanceVariable);
            }

            return null;
        }

        private static bool IsEnumerableElementMapping(IBasicMapperData mapperData)
            => mapperData.TargetMember.LeafMember.MemberType == MemberType.EnumerableElement;

        private static bool SourceAndTargetAreExactMatches(MemberMapperData mapperData)
            => mapperData.TargetMember.LeafMember.IsRoot || mapperData.SourceMember.Matches(mapperData.TargetMember);

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