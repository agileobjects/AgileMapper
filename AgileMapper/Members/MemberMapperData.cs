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
                  parent.TargetType,
                  targetMember,
                  parent)
        {
        }

        public MemberMapperData(
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

        public ObjectMapperData Parent { get; }

        public virtual ParameterExpression Parameter => Parent.Parameter;

        public virtual IQualifiedMember SourceMember => Parent.SourceMember;

        public virtual Expression SourceObject => Parent.SourceObject;

        public virtual Expression TargetObject => Parent.TargetObject;

        public virtual Expression EnumerableIndex => Parent.EnumerableIndex;

        public virtual ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public virtual NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}