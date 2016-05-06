namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;

    internal class MemberMappingContext : IMemberMappingContext
    {
        public MemberMappingContext(QualifiedMember targetMember, IMemberMappingContext parent)
        {
            TargetMember = targetMember;
            Parent = parent;
        }

        public IMemberMappingContext Parent { get; }

        public string RuleSetName => Parent.RuleSetName;

        public QualifiedMember TargetMember { get; }

        public Expression SourceObject => Parent.SourceObject;

        public Expression ExistingObject => Parent.ExistingObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public ParameterExpression TargetVariable => Parent.TargetVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}