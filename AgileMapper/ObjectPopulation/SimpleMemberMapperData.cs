namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using Members.Extensions;
    using Members.Sources;

    internal class SimpleMemberMapperData : MemberMapperDataBase, IMemberMapperData
    {
        private Expression _parentObject;
        private Expression _sourceObject;
        private Expression _targetObject;

        public SimpleMemberMapperData(
            IQualifiedMember sourceMember,
            IMemberMapperData memberMapperData)
            : base(
                memberMapperData.RuleSet,
                sourceMember,
                memberMapperData.TargetMember,
                memberMapperData.Parent,
                memberMapperData.MapperContext)
        {
            ElementIndex = Parent.ElementIndex;
            ElementKey = Parent.ElementKey;
        }

        private SimpleMemberMapperData(
            IQualifiedMember sourceElementMember,
            QualifiedMember targetElementMember,
            ObjectMapperData enumerableMapperData)
            : base(
                enumerableMapperData.RuleSet,
                sourceElementMember,
                targetElementMember,
                enumerableMapperData,
                enumerableMapperData.MapperContext)
        {
            ElementIndex = enumerableMapperData
                .EnumerablePopulationBuilder.Counter.GetConversionTo<int?>();

            ElementKey = enumerableMapperData
                .EnumerablePopulationBuilder.GetElementKey();
        }

        #region Factory Method

        public static SimpleMemberMapperData Create(Expression sourceValue, IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsEnumerable)
            {
                return new SimpleMemberMapperData(
                    sourceValue.ToSourceMember(mapperData.MapperContext),
                    mapperData);
            }

            var enumerableMapperData = (ObjectMapperData)mapperData;
            var membersSource = new ElementMembersSource(enumerableMapperData);

            return new SimpleMemberMapperData(
                membersSource.GetSourceMember().WithType(sourceValue.Type),
                membersSource.GetTargetMember(),
                enumerableMapperData);
        }

        #endregion

        public override bool IsEntryPoint => false;

        public MapperDataContext Context => null;

        public Expression ParentObject
            => _parentObject ??= GetParentObjectAccess();

        protected override Expression GetNestedSourceObject()
            => _sourceObject ??= GetMappingDataProperty("Source");

        protected override Expression GetNestedTargetObject()
            => _targetObject ??= GetMappingDataProperty("Target");

        public Expression TargetInstance => TargetObject;

        public ParameterExpression CreatedObject => null;

        public Expression ElementIndex { get; }

        public Expression ElementKey { get; }
    }
}