namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MemberMapperData : BasicMapperData
    {
        public MemberMapperData(QualifiedMember targetMember, ObjectMapperData parent)
            : this(
                  parent.MapperContext,
                  parent.RuleSet,
                  parent.SourceType,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
        }

        protected MemberMapperData(
            MapperContext mapperContext,
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType,
            QualifiedMember targetMember,
            ObjectMapperData parent)
            : base(ruleSet, sourceType, targetType, targetMember, parent)
        {
            MapperContext = mapperContext;
            Parent = parent;
        }

        public MapperContext MapperContext { get; }

        public new ObjectMapperData Parent { get; }

        public virtual ParameterExpression MdParameter => Parent.MdParameter;

        public virtual IQualifiedMember SourceMember => Parent.SourceMember;

        public virtual Expression SourceObject => Parent.SourceObject;

        public virtual Expression TargetObject => Parent.TargetObject;

        public virtual Expression EnumerableIndex => Parent.EnumerableIndex;

        public virtual ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public virtual NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}