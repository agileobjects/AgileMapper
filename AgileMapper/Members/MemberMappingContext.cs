namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MemberMappingContext : IMemberMappingContext
    {
        public MemberMappingContext(QualifiedMember targetMember, IObjectMappingContext parent)
        {
            TargetMember = targetMember;
            Parent = parent;
        }

        public IObjectMappingContext Parent { get; }

        public ParameterExpression Parameter => Parent.Parameter;

        public string RuleSetName => Parent.RuleSetName;

        public QualifiedMember TargetMember { get; }

        public Expression SourceObject => Parent.SourceObject;

        public int SourceObjectDepth => Parent.SourceObjectDepth;

        public Expression ExistingObject => Parent.ExistingObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}