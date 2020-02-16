namespace AgileObjects.AgileMapper.DataSources.Optimisation
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using DataSources;
    using Extensions.Internal;
    using Members;

    internal static class MultiInvocationsProcessor
    {
        public static void Process(
            IDataSource dataSource,
            IMemberMapperData mapperData,
            out Expression condition,
            out Expression value,
            out IList<ParameterExpression> variables)
        {
            condition = dataSource.Condition;
            value = dataSource.Value;

            if (SkipProcessing(value, mapperData))
            {
                variables = Enumerable<ParameterExpression>.EmptyArray;
                return;
            }

            var isUnconditional = condition == null;

            var populationExpressions = isUnconditional
                ? value
                : Expression.Block(condition, value);

            MultiInvocationsFinder.FindIn(
                populationExpressions,
                mapperData,
                out var multiInvocations,
                out variables);

            switch (multiInvocations.Count)
            {
                case 0:
                    return;

                case 1:
                    var valueVariable = variables[0];
                    var valueVariableValue = multiInvocations[0];
                    var valueVariableAssignment = valueVariable.AssignTo(valueVariableValue);

                    populationExpressions = populationExpressions
                        .Replace(
                            valueVariableValue,
                            valueVariable,
                            ExpressionEvaluation.Equivalator)
                        .ReplaceParameter(
                            valueVariable,
                            valueVariableAssignment,
                            replacementCount: 1);

                    goto SetPopulationExpressions;
            }

            populationExpressions = ProcessMultiInvocationsPopulation(
                multiInvocations,
                variables,
                populationExpressions);

            SetPopulationExpressions:

            if (isUnconditional)
            {
                value = populationExpressions;
                return;
            }

            var populationExpressionsBlock = (BlockExpression)populationExpressions;
            condition = populationExpressionsBlock.Expressions[0];
            value = populationExpressionsBlock.Expressions[1];
        }

        private static bool SkipProcessing(Expression value, IMemberMapperData mapperData)
        {
            switch (value.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return true;

                case ExpressionType.Try:
                    return mapperData.Parent
                        .AnyChildMapperDataMatches(cmd => cmd.Context.IsForDerivedType);
            }

            return false;
        }

        private static Expression ProcessMultiInvocationsPopulation(
            IList<Expression> multiInvocations,
            IList<ParameterExpression> variables,
            Expression populationExpressions)
        {
            var multiInvocationsCount = multiInvocations.Count;
            var cachedValuesCount = multiInvocationsCount - 1;

            var valueVariablesByInvocation = FixedSizeExpressionReplacementDictionary
                .WithEquivalentKeys(cachedValuesCount);

            var previousValueVariableAssignment = default(Expression);
            var previousInvocation = default(Expression);
            var previousValueVariable = default(ParameterExpression);

            for (var i = 0; i < multiInvocationsCount; ++i)
            {
                var invocation = multiInvocations[i];
                var valueVariable = variables[i];
                var isLastInvocation = i == cachedValuesCount;

                var valueVariableValue = invocation;

                for (int j = 0; j < i; ++j)
                {
                    valueVariableValue = valueVariableValue.Replace(
                        valueVariablesByInvocation.Keys[j],
                        valueVariablesByInvocation.Values[j],
                        ExpressionEvaluation.Equivalator);
                }

                if (!isLastInvocation)
                {
                    valueVariablesByInvocation.Add(valueVariableValue, valueVariable);
                }

                populationExpressions = populationExpressions.Replace(
                    valueVariableValue,
                    valueVariable,
                    ExpressionEvaluation.Equivalator);

                Expression valueVariableAssignment;

                if ((previousInvocation != null) && invocation.IsRootedIn(previousInvocation))
                {
                    var chainedValueVariableInvocation = valueVariableValue
                        .Replace(previousValueVariable, previousValueVariableAssignment);

                    valueVariableAssignment = valueVariable.AssignTo(chainedValueVariableInvocation);

                    var chainedAssignmentPopulation = populationExpressions.Replace(
                        chainedValueVariableInvocation,
                        valueVariableAssignment,
                        ExpressionEvaluation.Equivalator,
                        replacementCount: 1);

                    if (chainedAssignmentPopulation != populationExpressions)
                    {
                        populationExpressions = chainedAssignmentPopulation;

                        if (isLastInvocation)
                        {
                            return populationExpressions;
                        }

                        goto SetPreviousValues;
                    }
                }

                valueVariableAssignment = valueVariable.AssignTo(valueVariableValue);

                populationExpressions = populationExpressions.ReplaceParameter(
                    valueVariable,
                    valueVariableAssignment,
                    replacementCount: 1);

                SetPreviousValues:
                previousInvocation = invocation;
                previousValueVariable = valueVariable;
                previousValueVariableAssignment = valueVariableAssignment;
            }

            return populationExpressions;
        }
    }
}