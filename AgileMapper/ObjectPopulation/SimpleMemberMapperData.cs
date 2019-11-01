namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using Members.Sources;

    internal class SimpleMemberMapperData : MemberMapperDataBase, IMemberMapperData
    {
        private SimpleMemberMapperData(IMemberMapperData memberMapperData, Type sourceType)
            : base(
                memberMapperData.RuleSet,
                memberMapperData.SourceMember.WithType(sourceType),
                memberMapperData.TargetMember,
                memberMapperData.MapperContext,
                memberMapperData.Parent)
        {
            ParentObject = GetParentObjectAccess();
            EnumerableIndex = GetEnumerableIndexAccess();
            EnumerableIndexValue = Parent.EnumerableIndex;
        }

        private SimpleMemberMapperData(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData enumerableMapperData)
            : base(
                enumerableMapperData.RuleSet,
                sourceMember,
                targetMember,
                enumerableMapperData.MapperContext,
                enumerableMapperData)
        {
            ParentObject = GetParentObjectAccess();
            EnumerableIndex = GetEnumerableIndexAccess();
            EnumerableIndexValue = enumerableMapperData.EnumerablePopulationBuilder.Counter.GetConversionTo<int?>();
        }

        #region Factory Method

        public static SimpleMemberMapperData Create(Type sourceType, IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsEnumerable)
            {
                return new SimpleMemberMapperData(mapperData, sourceType);
            }

            var enumerableMapperData = (ObjectMapperData)mapperData;
            var membersSource = new ElementMembersSource(enumerableMapperData);

            return new SimpleMemberMapperData(
                membersSource.GetSourceMember().WithType(sourceType),
                membersSource.GetTargetMember(),
                enumerableMapperData);
        }

        #endregion

        public bool IsEntryPoint => false;

        public MapperDataContext Context => null;

        public Expression ParentObject { get; }

        public Expression CreatedObject => null;

        public Expression EnumerableIndex { get; }

        public Expression EnumerableIndexValue { get; }

        public Expression TargetInstance => TargetObject;

        public ExpressionInfoFinder ExpressionInfoFinder => Parent.ExpressionInfoFinder;
    }
}