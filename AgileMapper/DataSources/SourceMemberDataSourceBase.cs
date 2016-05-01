namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class SourceMemberDataSourceBase : IDataSource
    {
        protected SourceMemberDataSourceBase(Expression value, Expression sourceObject)
        {
            NestedSourceMemberAccesses = NestedSourceMemberAccessFinder.FindIn(value, sourceObject);
            Value = value;
        }

        public IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        public Expression Value { get; }

        #region Helper Class

        private class NestedSourceMemberAccessFinder : ExpressionVisitor
        {
            private readonly Expression _contextSourceParameter;
            private readonly Dictionary<string, Expression> _memberAccessesByPath;

            private NestedSourceMemberAccessFinder(Expression contextSourceParameter)
            {
                _contextSourceParameter = contextSourceParameter;
                _memberAccessesByPath = new Dictionary<string, Expression>();
            }

            public static IEnumerable<Expression> FindIn(Expression value, Expression sourceObject)
            {
                var visitor = new NestedSourceMemberAccessFinder(sourceObject);

                visitor.Visit(value);

                return visitor._memberAccessesByPath.Values;
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                AddMemberAccessIfAppropriate(memberAccess);

                return base.VisitMember(memberAccess);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
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
                return (memberAccess.Type != typeof(string)) &&
                       !_memberAccessesByPath.ContainsKey(memberAccess.ToString()) &&
                       memberAccess.Type.CanBeNull() &&
                       memberAccess.IsRootedIn(_contextSourceParameter);
            }
        }

        #endregion
    }
}