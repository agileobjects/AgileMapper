namespace AgileObjects.AgileMapper.DataSources.Optimisation
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using ReadableExpressions.Extensions;
    using Members;
    using Members.MemberExtensions;

    internal static class MultiInvocationsFinder
    {
        public static void FindIn(
            Expression value,
            IMemberMapperData mapperData,
            out IList<Expression> multiInvocations,
            out IList<ParameterExpression> variables)
        {
            var finder = new MultiInvocationsFinderInstance(mapperData);
            multiInvocations = finder.FindIn(value);

            switch (multiInvocations.Count)
            {
                case 0:
                    variables = Enumerable<ParameterExpression>.EmptyArray;
                    return;

                case 1:
                    variables = new[] { CreateVariableFor(multiInvocations[0]) };
                    return;

                default:
                    var multiInvocationsCount = multiInvocations.Count;
                    variables = new ParameterExpression[multiInvocationsCount];

                    for (var i = 0; i < multiInvocationsCount; ++i)
                    {
                        variables[i] = CreateVariableFor(multiInvocations[i]);
                    }
                    return;
            }
        }

        private static ParameterExpression CreateVariableFor(Expression invocation)
        {
            var valueVariableName = GetVariableNameFor(invocation);
            return Expression.Variable(invocation.Type, valueVariableName);
        }

        private static string GetVariableNameFor(Expression invocation)
        {
            var variableNameBase = (invocation.NodeType == ExpressionType.Call)
                ? ((MethodCallExpression)invocation).Method.Name
                : invocation.Type.Name;

            return variableNameBase.ToCamelCase() + "Result";
        }

        private class MultiInvocationsFinderInstance : ExpressionVisitor
        {
            private readonly Expression _rootMappingData;
            private ICollection<Expression> _allInvocations;
            private ICollection<Expression> _multiInvocations;

            public MultiInvocationsFinderInstance(IMemberMapperData mapperData)
            {
                _rootMappingData = mapperData.RootMappingDataObject;
            }

            private ICollection<Expression> AllInvocations
                => _allInvocations ?? (_allInvocations = new List<Expression>());

            private ICollection<Expression> MultiInvocations
                => _multiInvocations ?? (_multiInvocations = new List<Expression>());

            public IList<Expression> FindIn(Expression expression)
            {
                Visit(expression);

                if (_multiInvocations?.Any() != true)
                {
                    return Enumerable<Expression>.EmptyArray;
                }

                return _multiInvocations
                    .OrderBy(inv => inv.ToString())
                    .ToArray();
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (methodCall.IsMappingDataObjectCall(_rootMappingData))
                {
                    return base.VisitMethodCall(methodCall);
                }

                AddInvocation(methodCall);

                return base.VisitMethodCall(methodCall);
            }

            protected override Expression VisitInvocation(InvocationExpression invocation)
            {
                AddInvocation(invocation);

                return base.VisitInvocation(invocation);
            }

            private void AddInvocation(Expression invocation)
            {
                if (_allInvocations?.Contains(invocation) != true)
                {
                    AllInvocations.Add(invocation);
                }
                else if (_multiInvocations?.Contains(invocation) != true)
                {
                    MultiInvocations.Add(invocation);
                }
            }
        }
    }
}