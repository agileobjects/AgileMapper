namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using TypeConversion;
    using Members;

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

            var loopBody = GetLoopBody(loopData, builder, breakLoop, elementPopulation);

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

        private static BlockExpression GetLoopBody(
            IPopulationLoopData loopData,
            EnumerablePopulationBuilder builder,
            Expression breakLoop,
            Expression elementPopulation)
        {
            var ifExitCheckBreakLoop = Expression.IfThen(loopData.LoopExitCheck, breakLoop);
            var counterIncrement = builder.GetCounterIncrement();

            var sourceValueFilter = builder
                .MapperData
                .GetSourceValueFilterOrNull();

            if (sourceValueFilter != null)
            {
                elementPopulation = Expression.IfThen(
                    sourceValueFilter.GetConditionOrNull(loopData.GetSourceElementValue()),
                    elementPopulation);
            }

            if (elementPopulation.NodeType != ExpressionType.Block)
            {
                return Expression.Block(ifExitCheckBreakLoop, elementPopulation, counterIncrement);
            }

            var elementPopulationBlock = (BlockExpression)elementPopulation;

            var loopExpressions = new Expression[elementPopulationBlock.Expressions.Count + 2];

            loopExpressions[0] = ifExitCheckBreakLoop;
            loopExpressions.CopyFrom(elementPopulationBlock.Expressions, startIndex: 1);
            loopExpressions[loopExpressions.Length - 1] = counterIncrement;

            return elementPopulationBlock.Variables.Any()
                ? Expression.Block(elementPopulationBlock.Variables, loopExpressions)
                : Expression.Block(loopExpressions);
        }
    }
}