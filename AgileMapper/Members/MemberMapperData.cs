namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MemberMapperData : BasicMapperData, IMemberMapperData
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

        public ParameterExpression Parameter => Parent.Parameter;

        public IQualifiedMember SourceMember => Parent.SourceMember;

        public Expression SourceObject => Parent.SourceObject;

        public Expression TargetObject => Parent.TargetObject;

        public Expression EnumerableIndex => Parent.EnumerableIndex;

        public ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;
    }
}