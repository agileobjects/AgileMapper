namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using Extensions.Internal;
    using TypeConversion;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                Expression.IfThen(loopData.LoopExitCheck, breakLoop),
                elementPopulation,
                builder.GetCounterIncrement());

            var populationLoop = loopData.NeedsContinueTarget
                ? Expression.Loop(loopBody, breakLoop.Target, loopData.ContinueLoopTarget)
                : Expression.Loop(loopBody, breakLoop.Target);

            var adaptedLoop = loopData.Adapt(populationLoop);

            var population = Expression.Block(
                new[] { builder.Counter },
                builder.Counter.AssignTo(ToNumericConverter<int>.Zero),
                adaptedLoop);

            return population;
        }
    }
}