namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class NestedAccessFinder : ExpressionVisitor
    {
        private static readonly object _syncLock = new object();

        private readonly Expression _dataParameter;
        private readonly ICollection<Expression> _memberAccessSubjects;
        private readonly Dictionary<string, Expression> _memberAccessesByPath;

        private bool _includeSourceObjectAccesses;

        public NestedAccessFinder(Expression dataParameter)
        {
            _dataParameter = dataParameter;
            _memberAccessSubjects = new List<Expression>();
            _memberAccessesByPath = new Dictionary<string, Expression>();
        }

        public Expression[] FindIn(Expression expression, bool includeSourceObjectAccesses)
        {
            Expression[] memberAccesses;

            lock (_syncLock)
            {
                _includeSourceObjectAccesses = includeSourceObjectAccesses;

                Visit(expression);

                memberAccesses = _memberAccessesByPath.Values.Reverse().ToArray();

                _memberAccessSubjects.Clear();
                _memberAccessesByPath.Clear();
            }

            return memberAccesses;
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if (IsNotRootObject(memberAccess))
            {
                if ((memberAccess.Expression != null) && IsNotRootObject(memberAccess.Expression))
                {
                    _memberAccessSubjects.Add(memberAccess.Expression);
                }

                AddMemberAccessIfAppropriate(memberAccess);
            }

            return base.VisitMember(memberAccess);
        }

        private bool IsNotRootObject(Expression expression)
        {
            if (expression == _dataParameter)
            {
                return false;
            }

            return (expression.NodeType != ExpressionType.MemberAccess) ||
                   IsNotRootObject((MemberExpression)expression);
        }

        private bool IsNotRootObject(MemberExpression memberAccess)
        {
            if (memberAccess.Member.Name == "Parent")
            {
                return !memberAccess.IsRootedIn(_dataParameter);
            }

            if (memberAccess.Expression != _dataParameter)
            {
                return true;
            }

            if (memberAccess.Member.Name == "EnumerableIndex")
            {
                return false;
            }

            return _includeSourceObjectAccesses || (memberAccess.Member.Name != "Source");
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if ((methodCall.Object != null) && IsNotRootObject(methodCall.Object))
            {
                _memberAccessSubjects.Add(methodCall.Object);
            }

            if ((methodCall.Object != _dataParameter) &&
                (methodCall.Method.DeclaringType != typeof(IMappingData)))
            {
                AddMemberAccessIfAppropriate(methodCall);
            }

            return base.VisitMethodCall(methodCall);
        }

        private void AddMemberAccessIfAppropriate(Expression memberAccess)
        {
            if (Add(memberAccess))
            {
                _memberAccessesByPath.Add(memberAccess.ToString(), memberAccess);
            }
        }

        private bool Add(Expression memberAccess)
        {
            return ((memberAccess.Type != typeof(string)) || _memberAccessSubjects.Contains(memberAccess)) &&
                   !_memberAccessesByPath.ContainsKey(memberAccess.ToString()) &&
                   memberAccess.Type.CanBeNull() &&
                   memberAccess.IsRootedIn(_dataParameter);
        }
    }
}