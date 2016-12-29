namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal interface IPopulationLoopData
    {
        Expression LoopExitCheck { get; }

        Expression GetElementToAdd(IObjectMappingData enumerableMappingData);

        Expression Adapt(LoopExpression loop);
    }

    internal static class PopulationLoopDataExtensions
    {
        public static Expression BuildPopulationLoop<TArg>(
            this IPopulationLoopData loopData,
            EnumerablePopulationBuilder builder,
            TArg builderArgument,
            Func<IPopulationLoopData, TArg, Expression> mappedElementAdditionFactory)
        {
            var breakLoop = Expression.Break(Expression.Label(typeof(void), "Break"));
            var mappedElementAddition = mappedElementAdditionFactory.Invoke(loopData, builderArgument);

            var loopBody = Expression.Block(
                Expression.IfThen(loopData.LoopExitCheck, breakLoop),
                mappedElementAddition,
                Expression.PreIncrementAssign(builder.Counter));

            var populationLoop = Expression.Loop(loopBody, breakLoop.Target);
            var adaptedLoop = loopData.Adapt(populationLoop);

            var population = Expression.Block(
                new[] { builder.Counter },
                builder.Counter.AssignTo(0.ToConstantExpression()),
                adaptedLoop);

            return population;
        }
    }
}