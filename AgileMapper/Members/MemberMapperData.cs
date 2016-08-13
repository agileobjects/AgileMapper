namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
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

        public virtual ParameterExpression Parameter => Parent.Parameter;

        public virtual IQualifiedMember SourceMember => Parent.SourceMember;

        public virtual Expression SourceObject => Parent.SourceObject;

        public virtual Expression TargetObject => Parent.TargetObject;

        public virtual Expression EnumerableIndex => Parent.EnumerableIndex;

        public virtual ParameterExpression InstanceVariable => Parent.InstanceVariable;

        public virtual NestedAccessFinder NestedAccessFinder => Parent.NestedAccessFinder;

        public Expression GetSourceMemberAccess(IQualifiedMember childSourceMember, Expression instance = null)
        {
            var relativeMember = childSourceMember.RelativeTo(SourceMember);
            var memberAccess = relativeMember.GetQualifiedAccess(instance ?? SourceObject);

            if (RuleSet.SourceCanBeNull)
            {
                memberAccess = memberAccess.WithNullChecks(Parameter);
            }

            return memberAccess;
        }

        public Expression GetTargetMemberAccess(Expression instance = null)
        {
            var targetMemberAccess = TargetMember.GetAccess(instance ?? InstanceVariable);
            var checkedAccess = targetMemberAccess.WithNullChecks(InstanceVariable);

            return checkedAccess;
        }

        public MethodCallExpression GetCreateChildMappingDataCall(IQualifiedMember sourceMember, int dataSourceIndex)
        {
            var createChildMappingDataMethod = Parameter.Type
                .GetMethod("CreateChildMappingData", Constants.PublicInstance)
                .MakeGenericMethod(sourceMember.Type, TargetMember.Type);

            var sourceMemberValue = GetSourceMemberAccess(sourceMember);
            var targetMemberValue = GetTargetMemberAccess();

            var createChildMappingDataCall = Expression.Call(
                Parameter,
                createChildMappingDataMethod,
                sourceMemberValue,
                targetMemberValue,
                Expression.Constant(TargetMember.Name, typeof(string)),
                Expression.Constant(dataSourceIndex, typeof(int)));

            return createChildMappingDataCall;
        }
    }
}