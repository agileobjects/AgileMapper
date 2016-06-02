namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NestedAccessFinder : ExpressionVisitor
    {
        private readonly Expression _contextParameter;
        private readonly ICollection<Expression> _memberAccessSubjects;
        private readonly Dictionary<string, Expression> _memberAccessesByPath;

        public NestedAccessFinder(Expression contextParameter)
        {
            _contextParameter = contextParameter;
            _memberAccessSubjects = new List<Expression>();
            _memberAccessesByPath = new Dictionary<string, Expression>();
        }

        public IEnumerable<Expression> FindIn(Expression expression)
        {
            IEnumerable<Expression> memberAccesses;

            lock (this)
            {
                Visit(expression);

                memberAccesses = _memberAccessesByPath.Values.Reverse().ToArray();

                _memberAccessSubjects.Clear();
                _memberAccessesByPath.Clear();
            }

            return memberAccesses;
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if (IsNotRootSourceObject(memberAccess))
            {
                if ((memberAccess.Expression != null) && IsNotRootSourceObject(memberAccess.Expression))
                {
                    _memberAccessSubjects.Add(memberAccess.Expression);
                }

                AddMemberAccessIfAppropriate(memberAccess);
            }

            return base.VisitMember(memberAccess);
        }

        private bool IsNotRootSourceObject(Expression expression)
        {
            return (expression.NodeType != ExpressionType.MemberAccess) ||
                   IsNotRootSourceObject((MemberExpression)expression);
        }

        private bool IsNotRootSourceObject(MemberExpression memberAccess)
            => !(memberAccess.Member.Name == "Source" && memberAccess.Expression == _contextParameter);

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if ((methodCall.Object != null) && IsNotRootSourceObject(methodCall.Object))
            {
                _memberAccessSubjects.Add(methodCall.Object);
            }

            if (methodCall.Object != _contextParameter)
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
                   memberAccess.IsRootedIn(_contextParameter);
        }
    }
}