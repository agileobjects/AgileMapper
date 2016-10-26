namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IMemberMapperData : IBasicMapperData
    {
        MapperContext MapperContext { get; }

        new ObjectMapperData Parent { get; }

        Expression ParentObject { get; }

        ParameterExpression MappingDataObject { get; }

        IQualifiedMember SourceMember { get; }

        Expression SourceObject { get; }

        Expression TargetObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression InstanceVariable { get; }

        NestedAccessFinder NestedAccessFinder { get; }
    }

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
        }

        public MapperContext MapperContext => Parent.MapperContext;

        public ObjectMapperData Parent { get; }

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