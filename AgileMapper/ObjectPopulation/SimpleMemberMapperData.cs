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
        public SimpleMemberMapperData(IQualifiedMember sourceMember, IMemberMapperData memberMapperData)
            : base(
                memberMapperData.RuleSet,
                sourceMember,
                memberMapperData.TargetMember,
                memberMapperData.Parent,
                memberMapperData.MapperContext)
        {
            ParentObject = GetParentObjectAccess();
            ElementIndex = GetElementIndexAccess();
            ElementKey = GetElementKeyAccess();
            ElementIndexValue = Parent.ElementIndex;
        }

        private SimpleMemberMapperData(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData enumerableMapperData)
            : base(
                enumerableMapperData.RuleSet,
                sourceMember,
                targetMember,
                enumerableMapperData,
                enumerableMapperData.MapperContext)
        {
            ParentObject = GetParentObjectAccess();
            ElementIndex = GetElementIndexAccess();
            ElementKey = GetElementKeyAccess();
            ElementIndexValue = enumerableMapperData.EnumerablePopulationBuilder.Counter.GetConversionTo<int?>();
        }

        #region Factory Method

        public static SimpleMemberMapperData Create(Expression sourceValue, IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsEnumerable)
            {
                return new SimpleMemberMapperData(sourceValue.ToSourceMember(mapperData.MapperContext), mapperData);
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

        public Expression ParentObject { get; }

        public Expression RootMappingDataObject => Parent.RootMappingDataObject;

        public Expression CreatedObject => null;

        public Expression ElementIndex { get; }

        public Expression ElementKey { get; }

        public Expression ElementIndexValue { get; }

        public Expression TargetInstance => TargetObject;
    }
}