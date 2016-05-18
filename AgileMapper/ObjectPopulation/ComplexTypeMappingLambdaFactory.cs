namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ComplexTypeMappingLambdaFactory<TSource, TTarget, TInstance>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance> Instance =
            new ComplexTypeMappingLambdaFactory<TSource, TTarget, TInstance>();

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, omc);
            yield return GetExistingObjectShortCircuit(returnNull.Target, omc);
        }

        private static Expression GetStrategyShortCircuitReturns(Expression returnNull, IObjectMappingContext omc)
        {
            var matchingSourceMemberDataSource = omc
                .MapperContext
                .DataSources
                .GetSourceMemberDataSourceOrNull(omc);

            if (matchingSourceMemberDataSource == null)
            {
                return Constants.EmptyExpression;
            }

            Expression sourceObject;
            Func<IEnumerable<Expression>, Expression> blockBuilder;

            if (matchingSourceMemberDataSource.Value == omc.SourceObject)
            {
                sourceObject = omc.SourceObject;
                blockBuilder = Expression.Block;
            }
            else
            {
                sourceObject = Expression.Variable(matchingSourceMemberDataSource.Value.Type, "matchingSource");
                Expression assignSourceObject = Expression.Assign(sourceObject, matchingSourceMemberDataSource.Value);

                blockBuilder = conditions => Expression.Block(
                    new[] { (ParameterExpression)sourceObject },
                    new[] { assignSourceObject }.Concat(conditions));
            }

            var shortCircuitConditions = omc.MappingContext
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(sourceObject, omc)
                .Select(condition => Expression.IfThen(condition, returnNull))
                .ToArray();

            var shortCircuitBlock = blockBuilder.Invoke(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static Expression GetExistingObjectShortCircuit(LabelTarget returnTarget, IObjectMappingContext omc)
        {
            var ifTryGetReturn = Expression.IfThen(
                omc.TryGetCall,
                Expression.Return(returnTarget, omc.InstanceVariable));

            return ifTryGetReturn;
        }

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
            => Expression.Coalesce(omc.ExistingObject, omc.CreateCall);

        protected override IEnumerable<Expression> GetObjectPopulation(Expression instanceVariableValue, IObjectMappingContext omc)
        {
            var objectRegistration = omc.ObjectRegistrationCall;
            var objectCreationCallback = GetObjectCreatedCallback(omc);
            var memberPopulations = MemberPopulationFactory.Create(omc);

            var successfulPopulations = memberPopulations
                .Where(p => p.IsSuccessful)
                .Select(p => p.GetPopulation())
                .ToArray();

            var unsuccessfulPopulations = memberPopulations
                .Where(p => !p.IsSuccessful)
                .Select(p => p.GetPopulation())
                .ToArray();

            return new[] { objectRegistration, objectCreationCallback }
                .Concat(successfulPopulations)
                .Concat(unsuccessfulPopulations)
                .ToArray();
        }

        private static Expression GetObjectCreatedCallback(IObjectMappingContext omc)
        {
            ObjectCreationCallback callback;

            if (omc.MapperContext.UserConfigurations.HasCreationCallback(omc, out callback))
            {
                return callback.IntegrateCallback(omc);
            }

            return Constants.EmptyExpression;
        }

        protected override Expression GetReturnValue(Expression instanceVariableValue, IObjectMappingContext omc)
            => omc.InstanceVariable;
    }
}