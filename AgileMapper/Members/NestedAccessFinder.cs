namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

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
            return (expression.NodeType != ExpressionType.MemberAccess) ||
                   IsNotRootObject((MemberExpression)expression);
        }

        private bool IsNotRootObject(MemberExpression memberAccess)
        {
            if (memberAccess.Member.Name == "Parent")
            {
                return !memberAccess.IsRootedIn(_contextParameter);
            }

            return (memberAccess.Expression != _contextParameter) ||
                ((memberAccess.Member.Name != "Source") &&
                (memberAccess.Member.Name != "EnumerableIndex"));
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if ((methodCall.Object != null) && IsNotRootObject(methodCall.Object))
            {
                _memberAccessSubjects.Add(methodCall.Object);
            }

            if ((methodCall.Object != _contextParameter) &&
                (methodCall.Method.DeclaringType != typeof(IObjectMappingContext)))
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