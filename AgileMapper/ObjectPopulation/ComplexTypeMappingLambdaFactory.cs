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

        protected override IEnumerable<Expression> GetShortCircuitReturns(
            Expression returnNull,
            IObjectMappingContext omc)
        {
            var matchingSourceObject = omc
                .MapperContext
                .DataSources
                .FindBestMatchFor(omc.TargetMember, omc);

            if (matchingSourceObject == null)
            {
                yield return Expression.Empty();
                yield break;
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

            yield return shortCircuitBlock;
        }

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var existingObjectOrCreate = Expression
                .Coalesce(omc.ExistingObject, omc.GetCreateCall());

            return existingObjectOrCreate;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(Expression targetVariableValue, IObjectMappingContext omc)
        {
            var memberPopulations = MemberPopulationFactory
               .Create(omc)
               .Where(p => p.IsSuccessful)
               .ToArray();

            var processedPopulations = omc
                .MappingContext
                .RuleSet
                .Process(memberPopulations)
                .Select(d => d.Population)
                .ToArray();

            return processedPopulations;
        }

        protected override Expression GetReturnValue(Expression targetVariableValue, IObjectMappingContext omc)
        {
            return omc.TargetVariable;
        }
    }
}