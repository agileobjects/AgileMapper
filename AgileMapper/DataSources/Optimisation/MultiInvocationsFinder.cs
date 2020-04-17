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
            private List<Expression> _allInvocations;
            private List<Expression> _multiInvocations;

            public MultiInvocationsFinderInstance(IMemberMapperData mapperData)
            {
                _rootMappingData = mapperData.RootMappingDataObject;
            }

            private ICollection<Expression> AllInvocations
                => _allInvocations ??= new List<Expression>();

            private ICollection<Expression> MultiInvocations
                => _multiInvocations ??= new List<Expression>();

            public IList<Expression> FindIn(Expression expression)
            {
                Visit(expression);

                switch (_multiInvocations?.Count)
                {
                    case null:
                    case 0:
                        return Enumerable<Expression>.EmptyArray;

                    case 1:
                        return _multiInvocations;

                    default:
                        return _multiInvocations
                            .OrderBy(inv => inv.ToString())
                            .ToArray();
                }
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
                if (InvocationDoesNotExist(invocation, _allInvocations))
                {
                    AllInvocations.Add(invocation);
                }
                else if (InvocationDoesNotExist(invocation, _multiInvocations))
                {
                    MultiInvocations.Add(invocation);
                }
            }

            private static bool InvocationDoesNotExist(Expression invocation, ICollection<Expression> invocations)
            {
                if (invocations == null)
                {
                    return true;
                }

                if (invocations.Contains(invocation))
                {
                    return false;
                }
#if NET35
                if (invocations.Contains(invocation, CapturedInstanceMethodCallComparer.Instance))
                {
                    return false;
                }
#endif
                return true;
            }
#if NET35
            private class CapturedInstanceMethodCallComparer : IEqualityComparer<Expression>
            {
                public static readonly IEqualityComparer<Expression> Instance =
                    new CapturedInstanceMethodCallComparer();

                public bool Equals(Expression x, Expression y)
                {
                    if (x?.NodeType != ExpressionType.Call || y?.NodeType != ExpressionType.Call)
                    {
                        return false;
                    }

                    var methodCallX = (MethodCallExpression)x;

                    if (methodCallX.Method.IsStatic)
                    {
                        return false;
                    }

                    var methodCallY = (MethodCallExpression)y;

                    return (methodCallX.Method == methodCallY.Method) &&
                           (methodCallX.Object.NodeType == ExpressionType.Constant) &&
                           (methodCallY.Object.NodeType == ExpressionType.Constant) &&
                            ExpressionEvaluation.AreEqual(methodCallX.Object, methodCallY.Object);
                }

                public int GetHashCode(Expression obj) => 0;
            }
#endif
        }
    }
}