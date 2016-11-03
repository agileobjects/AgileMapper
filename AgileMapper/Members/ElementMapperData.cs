namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class ElementMapperData : BasicMapperData, IMemberMapperData
    {
        public ElementMapperData(
            Expression sourceElement,
            Expression targetElement,
            ObjectMapperData enumerableMapperData)
            : this(
                sourceElement,
                targetElement,
                enumerableMapperData.SourceMember.GetElementMember(),
                enumerableMapperData.TargetMember.GetElementMember(),
                enumerableMapperData)
        {
        }

        private ElementMapperData(
            Expression sourceElement,
            Expression targetElement,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData enumerableMapperData)
            : base(
                enumerableMapperData.RuleSet,
                sourceElement.Type,
                targetElement.Type,
                targetMember,
                enumerableMapperData)
        {
            Parent = enumerableMapperData;
            SourceObject = sourceElement;
            TargetObject = targetElement;
            SourceMember = sourceMember;
            IsForInlineMapping = this.IsForInlineMapping();
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public ObjectMapperData Parent { get; }

        public bool IsForInlineMapping { get; }

        public Expression ParentObject => Parent.ParentObject;

        public ParameterExpression MappingDataObject => Parent.MappingDataObject;

        public IQualifiedMember SourceMember { get; }

        public Expression SourceObject { get; }

        public Expression TargetObject { get; }

        public Expression EnumerableIndex => Parameters.EnumerableIndex;

        public ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}