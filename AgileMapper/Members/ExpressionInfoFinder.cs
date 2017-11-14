namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ExpressionInfoFinder
    {
        private readonly Expression _mappingDataObject;

        public ExpressionInfoFinder(Expression mappingDataObject)
        {
            _mappingDataObject = mappingDataObject;
        }

        public ExpressionInfo FindIn(Expression expression, bool targetCanBeNull)
        {
            var finder = new ExpressionInfoFinderInstance(_mappingDataObject, targetCanBeNull);
            var info = finder.FindIn(expression);

            return info;
        }

        private class ExpressionInfoFinderInstance : ExpressionVisitor
        {
            private readonly Expression _mappingDataObject;
            private readonly ICollection<Expression> _stringMemberAccessSubjects;
            private readonly ICollection<Expression> _allInvocations;
            private readonly ICollection<Expression> _multiInvocations;
            private readonly ICollection<string> _nullCheckSubjects;
            private readonly Dictionary<string, Expression> _nestedAccessesByPath;
            private readonly bool _includeTargetNullChecking;

            public ExpressionInfoFinderInstance(Expression mappingDataObject, bool targetCanBeNull)
            {
                _mappingDataObject = mappingDataObject;
                _stringMemberAccessSubjects = new List<Expression>();
                _allInvocations = new List<Expression>();
                _multiInvocations = new List<Expression>();
                _nullCheckSubjects = new List<string>();
                _nestedAccessesByPath = new Dictionary<string, Expression>();
                _includeTargetNullChecking = targetCanBeNull;
            }

            public ExpressionInfo FindIn(Expression expression)
            {
                Visit(expression);

                var nestedAccesses = _nestedAccessesByPath.None()
                    ? Enumerable<Expression>.EmptyArray
                    : _nestedAccessesByPath.Values.Reverse().ToArray();

                var multiInvocations = _multiInvocations
                    .OrderBy(inv => inv.ToString())
                    .ToArray();

                return new ExpressionInfo(nestedAccesses, multiInvocations);
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                if (TryGetNullComparison(binary.Left, binary.Right, binary, out var comparedValue) ||
                    TryGetNullComparison(binary.Right, binary.Left, binary, out comparedValue))
                {
                    AddExistingNullCheck(comparedValue);
                }

                return base.VisitBinary(binary);
            }

            private bool TryGetNullComparison(
                Expression comparedOperand,
                Expression nullOperand,
                Expression binary,
                out Expression comparedValue)
            {
                if ((binary.NodeType != ExpressionType.Equal) && (binary.NodeType != ExpressionType.NotEqual))
                {
                    comparedValue = null;
                    return false;
                }

                if ((nullOperand.NodeType != ExpressionType.Constant) || (((ConstantExpression)nullOperand).Value != null))
                {
                    comparedValue = null;
                    return false;
                }

                comparedValue = AddMemberAccess(comparedOperand) ? comparedOperand : null;
                return comparedValue != null;
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                if (IsRootObject(memberAccess))
                {
                    return base.VisitMember(memberAccess);
                }

                if (IsNullableHasValueAccess(memberAccess))
                {
                    AddExistingNullCheck(memberAccess.Expression);
                }

                AddStringMemberAccessSubjectIfAppropriate(memberAccess.Expression);
                AddMemberAccessIfAppropriate(memberAccess);

                return base.VisitMember(memberAccess);
            }

            private bool IsRootObject(MemberExpression memberAccess) => !IsNotRootObject(memberAccess);

            private bool IsNotRootObject(MemberExpression memberAccess)
            {
                if (memberAccess.Member.Name == "Parent")
                {
                    return !memberAccess.IsRootedIn(_mappingDataObject);
                }

                if (memberAccess.Expression != _mappingDataObject)
                {
                    return true;
                }

                if (memberAccess.Member.Name == "EnumerableIndex")
                {
                    return false;
                }

                if (memberAccess.Member.Name == "Source")
                {
                    return false;
                }

                return _includeTargetNullChecking || (memberAccess.Member.Name != "Target");
            }

            private static bool IsNullableHasValueAccess(MemberExpression memberAccess)
            {
                return (memberAccess.Expression != null) &&
                       (memberAccess.Member.Name == "HasValue") &&
                       (memberAccess.Expression.Type.IsNullableType());
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if ((methodCall.Object != _mappingDataObject) &&
                    (methodCall.Method.DeclaringType != typeof(IMappingData)))
                {
                    AddStringMemberAccessSubjectIfAppropriate(methodCall.Object);
                    AddInvocationIfNecessary(methodCall);
                    AddMemberAccessIfAppropriate(methodCall);
                }

                return base.VisitMethodCall(methodCall);
            }

            private void AddExistingNullCheck(Expression checkedAccess)
            {
                _nullCheckSubjects.Add(checkedAccess.ToString());
            }

            private void AddStringMemberAccessSubjectIfAppropriate(Expression member)
            {
                if ((member?.Type == typeof(string)) && AccessSubjectCouldBeNull(member))
                {
                    _stringMemberAccessSubjects.Add(member);
                }
            }

            private bool AccessSubjectCouldBeNull(Expression expression)
            {
                Expression subject;
                MemberExpression memberAccess;

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    memberAccess = (MemberExpression)expression;
                    subject = memberAccess.Expression;
                }
                else
                {
                    memberAccess = null;
                    subject = ((MethodCallExpression)expression).Object;
                }

                if (subject == null)
                {
                    return false;
                }

                if (subject.Type.CannotBeNull())
                {
                    return false;
                }

                return (expression.NodeType != ExpressionType.MemberAccess) ||
                       IsNotRootObject(memberAccess);
            }

            protected override Expression VisitInvocation(InvocationExpression invocation)
            {
                AddInvocationIfNecessary(invocation);

                return base.VisitInvocation(invocation);
            }

            private void AddInvocationIfNecessary(Expression invocation)
            {
                if (!_allInvocations.Contains(invocation))
                {
                    _allInvocations.Add(invocation);
                }
                else if (!_multiInvocations.Contains(invocation))
                {
                    _multiInvocations.Add(invocation);
                }
            }

            private void AddMemberAccessIfAppropriate(Expression memberAccess)
            {
                if (AddMemberAccess(memberAccess))
                {
                    _nestedAccessesByPath.Add(memberAccess.ToString(), memberAccess);
                }
            }

            private bool AddMemberAccess(Expression memberAccess)
            {
                if (memberAccess.Type.CannotBeNull() || !memberAccess.IsRootedIn(_mappingDataObject))
                {
                    return false;
                }

                if ((memberAccess.Type == typeof(string)) && !_stringMemberAccessSubjects.Contains(memberAccess))
                {
                    return false;
                }

                var memberAccessString = memberAccess.ToString();

                return !_nullCheckSubjects.Contains(memberAccessString) &&
                       !_nestedAccessesByPath.ContainsKey(memberAccessString);
            }
        }

        public class ExpressionInfo
        {
            public static readonly ExpressionInfo Empty =
                new ExpressionInfo(Enumerable<Expression>.EmptyArray, Enumerable<Expression>.EmptyArray);

            public ExpressionInfo(
                IList<Expression> nestedAccesses,
                IList<Expression> multiInvocations)
            {
                NestedAccesses = nestedAccesses;
                MultiInvocations = multiInvocations;
            }

            public IList<Expression> NestedAccesses { get; }

            public IList<Expression> MultiInvocations { get; }
        }
    }
}