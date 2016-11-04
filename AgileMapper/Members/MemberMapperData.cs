namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class ChildMemberMapperData : BasicMapperData, IMemberMapperData
    {
        public ChildMemberMapperData(QualifiedMember targetMember, ObjectMapperData parent)
            : base(parent.RuleSet, parent.SourceType, parent.TargetType, targetMember, parent)
        {
            Parent = parent;
            IsForStandaloneMapping = this.IsForStandaloneMapping();
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public ObjectMapperData Parent { get; }

        public bool IsForStandaloneMapping { get; }

        public Expression ParentObject => Parent.ParentObject;

        public ParameterExpression MappingDataObject => Parent.MappingDataObject;

        public IQualifiedMember SourceMember => Parent.SourceMember;

        public Expression SourceObject => Parent.SourceObject;

        public Expression TargetObject => Parent.TargetObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}