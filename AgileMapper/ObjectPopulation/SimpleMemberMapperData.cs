namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class SimpleMemberMapperData : MemberMapperDataBase, IMemberMapperData
    {
        public SimpleMemberMapperData(
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            MapperContext mapperContext,
            ObjectMapperData parent)
            : base(
                ruleSet,
                sourceMember,
                targetMember,
                mapperContext,
                parent)
        {
            ParentObject = GetParentObjectAccess();
            EnumerableIndex = GetEnumerableIndexAccess();
        }

        public bool IsEntryPoint => false;

        public MapperDataContext Context => null;

        public Expression ParentObject { get; }

        public Expression CreatedObject => null;

        public Expression EnumerableIndex { get; }

        public Expression TargetInstance => TargetObject;

        public ExpressionInfoFinder ExpressionInfoFinder => Parent.ExpressionInfoFinder;
    }
}