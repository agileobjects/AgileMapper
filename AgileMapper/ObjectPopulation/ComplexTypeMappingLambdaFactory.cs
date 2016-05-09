namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ComplexTypeMappingLambdaFactory<TSource, TTarget>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget> Instance =
            new ComplexTypeMappingLambdaFactory<TSource, TTarget>();

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
        {
            yield return GetStrategyShortCircuitReturns(returnNull, omc);
            yield return GetExistingObjectShortCircuit(returnNull.Target, omc);
        }

        private static Expression GetStrategyShortCircuitReturns(
            Expression returnNull,
            IObjectMappingContext omc)
        {
            var matchingSourceObject = omc
                .MapperContext
                .DataSources
                .FindBestMatchFor(omc.TargetMember, omc);

            if (matchingSourceObject == null)
            {
                return Constants.EmptyExpression;
            }

            Expression sourceObject;
            Func<IEnumerable<Expression>, Expression> builder;

            if (matchingSourceObject.Value == omc.SourceObject)
            {
                sourceObject = omc.SourceObject;
                builder = Expression.Block;
            }
            else
            {
                sourceObject = Expression.Variable(matchingSourceObject.Value.Type, "matchingSource");
                Expression assignSourceObject = Expression.Assign(sourceObject, matchingSourceObject.Value);

                builder = conditions => Expression.Block(
                    new[] { (ParameterExpression)sourceObject },
                    new[] { assignSourceObject }.Concat(conditions));
            }

            var shortCircuitConditions = omc.MappingContext
                .RuleSet
                .ComplexTypeMappingShortCircuitStrategy
                .GetConditions(sourceObject, omc)
                .Select(condition => Expression.IfThen(condition, returnNull))
                .ToArray();

            var shortCircuitBlock = builder.Invoke(shortCircuitConditions);

            return shortCircuitBlock;
        }

        private static Expression GetExistingObjectShortCircuit(
            LabelTarget returnTarget,
            IObjectMappingContext omc)
        {
            var ifTryGetReturn = Expression.IfThen(
                omc.GetTryGetCall(),
                Expression.Return(returnTarget, omc.TargetVariable));

            return ifTryGetReturn;
        }

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var existingObjectOrCreate = Expression
                .Coalesce(omc.ExistingObject, omc.GetCreateCall());

            return existingObjectOrCreate;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(Expression targetVariableValue, IObjectMappingContext omc)
        {
            var objectRegistration = omc.GetObjectRegistrationCall();
            var objectCreationCallback = GetObjectCreatedCallback(omc);
            var memberPopulations = MemberPopulationFactory.Create(omc);

            var successfulPopulations = memberPopulations
                .Where(p => p.IsSuccessful)
                .ToArray();

            var unsuccessfulPopulations = memberPopulations
                .Except(successfulPopulations)
                .Select(p => p.GetPopulation())
                .ToArray();

            var processedPopulations = omc
                .MappingContext
                .RuleSet
                .Process(successfulPopulations)
                .Select(d => d.GetPopulation())
                .ToArray();

            return new[] { objectRegistration, objectCreationCallback }
                .Concat(unsuccessfulPopulations)
                .Concat(processedPopulations)
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

        protected override Expression GetReturnValue(Expression targetVariableValue, IObjectMappingContext omc)
        {
            return omc.TargetVariable;
        }
    }
}