﻿namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal static class PopulationLoopDataExtensions
    {
        public static Expression BuildPopulationLoop<TLoopData>(
            this TLoopData loopData,
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData,
            Func<TLoopData, IObjectMappingData, Expression> elementPopulationFactory)
            where TLoopData : IPopulationLoopData
        {
            // TODO: Not all enumerable mappings require the Counter
            var breakLoop = Expression.Break(Expression.Label(typeof(void), "Break"));

            var elementPopulation = elementPopulationFactory.Invoke(loopData, mappingData);

            var loopBody = Expression.Block(
                loopData.GetLoopGuard(breakLoop),
                elementPopulation,
                builder.GetCounterIncrement());

            var populationLoop = loopData.NeedsContinueTarget
                ? Expression.Loop(loopBody, breakLoop.Target, loopData.ContinueLoopTarget)
                : Expression.Loop(loopBody, breakLoop.Target);

            var adaptedLoop = loopData.Adapt(populationLoop);

            var population = Expression.Block(
                new[] { builder.Counter },
                builder.Counter.AssignTo(0.ToConstantExpression()),
                adaptedLoop);

            return population;
        }
    }
}