namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ComplexTypeMappingLambdaFactory : ObjectMappingLambdaFactoryBase
    {
        private readonly ComplexTypeConstructionFactory _constructionFactory;

        public ComplexTypeMappingLambdaFactory(MapperContext mapperContext)
        {
            _constructionFactory = new ComplexTypeConstructionFactory(mapperContext);
        }

        protected override bool IsNotConstructable(IObjectMapperCreationData data)
            => _constructionFactory.GetNewObjectCreation(data) == null;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMapperCreationData data)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, data.MapperData);
            yield return GetExistingObjectShortCircuit(returnNull.Target, data);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, MemberMapperData data)
        {
            if (!data.SourceMember.Matches(data.TargetMember))
            {
                return Constants.EmptyExpression;
            }

            var shortCircuitConditions = data
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(data)
                .Select(condition => (Expression)Expression.IfThen(condition, returnNull));

            var shortCircuitBlock = Expression.Block(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private Expression GetExistingObjectShortCircuit(LabelTarget returnTarget, IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            var objectCreation = GetObjectCreation(data);

            var objectCreationLambda = Expression.Lambda(
                Expression.GetFuncType(mapperData.InstanceVariable.Type),
                objectCreation);

            var tryGetOrRegisterMethod = MappingContext
                .TryGetOrRegisterMethod
                .MakeGenericMethod(mapperData.SourceType, mapperData.InstanceVariable.Type);

            var tryGetOrRegisterCall = Expression.Call(
                Expression.Property(mapperData.Parameter, "MappingContext"),
                tryGetOrRegisterMethod,
                mapperData.SourceObject,
                mapperData.InstanceVariable,
                objectCreationLambda);

            var ifTryGetReturn = Expression.IfThen(
                tryGetOrRegisterCall,
                Expression.Return(returnTarget, mapperData.InstanceVariable));

            return ifTryGetReturn;
        }

        private Expression GetObjectCreation(IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            var preCreationCallback = GetCreationCallbackOrEmpty(CallbackPosition.Before, mapperData);

            var instanceResolution = GetObjectResolution(data);

            var postCreationCallback = GetCreationCallbackOrEmpty(CallbackPosition.After, mapperData);

            if ((preCreationCallback == Constants.EmptyExpression) &&
                (postCreationCallback == Constants.EmptyExpression))
            {
                return instanceResolution;
            }

            var cacheFactoryVariable = Expression.Variable(mapperData.InstanceVariable.Type, "instance");
            var cacheFactoryAssignment = Expression.Assign(cacheFactoryVariable, instanceResolution);

            if (postCreationCallback != Constants.EmptyExpression)
            {
                postCreationCallback = postCreationCallback
                    .Replace(mapperData.InstanceVariable, mapperData.CreatedObject);
            }

            return Expression.Block(
                new[] { cacheFactoryVariable },
                preCreationCallback,
                cacheFactoryAssignment,
                postCreationCallback,
                cacheFactoryVariable);
        }

        private static Expression GetCreationCallbackOrEmpty(CallbackPosition callbackPosition, MemberMapperData data)
            => GetCallbackOrEmpty(c => c.GetCreationCallbackOrNull(callbackPosition, data), data);

        private Expression GetObjectResolution(IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            var objectCreation = _constructionFactory.GetNewObjectCreation(data);
            var createdObjectAssignment = Expression.Assign(mapperData.CreatedObject, objectCreation);
            var instanceDataTargetAssignment = Expression.Assign(mapperData.TargetObject, createdObjectAssignment);
            var existingOrCreatedObject = Expression.Coalesce(mapperData.TargetObject, instanceDataTargetAssignment);

            return existingOrCreatedObject;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMapperCreationData data)
        {
            var sourceMemberTypeTests = new List<Expression>();

            foreach (var memberPopulation in MemberPopulationFactory.Create(data))
            {
                if (!memberPopulation.IsSuccessful)
                {
                    yield return memberPopulation.GetPopulation();
                    continue;
                }

                var prePopulationCallback = GetPopulationCallbackOrEmpty(CallbackPosition.Before, memberPopulation, data);

                if (prePopulationCallback != Constants.EmptyExpression)
                {
                    yield return prePopulationCallback;
                }

                yield return memberPopulation.GetPopulation();

                var postPopulationCallback = GetPopulationCallbackOrEmpty(CallbackPosition.After, memberPopulation, data);

                if (postPopulationCallback != Constants.EmptyExpression)
                {
                    yield return postPopulationCallback;
                }

                if (memberPopulation.SourceMemberTypeTest != null)
                {
                    sourceMemberTypeTests.Add(memberPopulation.SourceMemberTypeTest);
                }
            }

            CreateSourceMemberTypeTesterIfRequired(sourceMemberTypeTests, data);
        }
        private static Expression GetPopulationCallbackOrEmpty(
            CallbackPosition position,
            IMemberPopulation memberPopulation,
            IObjectMapperCreationData data)
        {
            return GetCallbackOrEmpty(
                c => c.GetCallbackOrNull(position, memberPopulation.MapperData, data.MapperData),
                data.MapperData);
        }

        private static void CreateSourceMemberTypeTesterIfRequired(ICollection<Expression> typeTests, IObjectMapperCreationData data)
        {
            if (typeTests.None())
            {
                return;
            }

            var typeTest = typeTests.AndTogether();
            var typeTestLambda = Expression.Lambda<Func<IMappingData, bool>>(typeTest, Parameters.MappingData);

            data.MapperData.MapperKey.AddSourceMemberTypeTester(typeTestLambda.Compile());
        }

        protected override Expression GetReturnValue(ObjectMapperData data) => data.InstanceVariable;

        public void Reset() => _constructionFactory.Reset();
    }
}