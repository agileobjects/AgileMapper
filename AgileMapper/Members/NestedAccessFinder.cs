namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NestedAccessFinder : ExpressionVisitor
    {
        private readonly Expression _contextSourceParameter;
        private readonly ICollection<Expression> _methodCallSubjects;
        private readonly Dictionary<string, Expression> _memberAccessesByPath;

        public NestedAccessFinder(Expression contextSourceParameter)
        {
            _contextSourceParameter = contextSourceParameter;
            _methodCallSubjects = new List<Expression>();
            _memberAccessesByPath = new Dictionary<string, Expression>();
        }

        public IEnumerable<Expression> FindIn(Expression value)
        {
            IEnumerable<Expression> memberAccesses;

            lock (this)
            {
                Visit(value);

                memberAccesses = _memberAccessesByPath.Values.Reverse().ToArray();

                _methodCallSubjects.Clear();
                _memberAccessesByPath.Clear();
            }

            return memberAccesses;
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            AddMemberAccessIfAppropriate(memberAccess);

            return base.VisitMember(memberAccess);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Object != null)
            {
                _methodCallSubjects.Add(methodCall.Object);
            }

            AddMemberAccessIfAppropriate(methodCall);

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
            return ((memberAccess.Type != typeof(string)) || _methodCallSubjects.Contains(memberAccess)) &&
                   !_memberAccessesByPath.ContainsKey(memberAccess.ToString()) &&
                   memberAccess.Type.CanBeNull() &&
                   memberAccess.IsRootedIn(_contextSourceParameter);
        }
    }
}