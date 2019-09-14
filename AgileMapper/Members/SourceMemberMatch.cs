namespace AgileObjects.AgileMapper.Members
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions.Internal;

    internal class SourceMemberMatch
    {
        public static readonly SourceMemberMatch Null = new SourceMemberMatch();

        private SourceMemberMatch()
        {
        }

        public SourceMemberMatch(
            IQualifiedMember sourceMember,
            IChildMemberMappingData contextMappingData,
            Expression condition,
            bool isUseable = true)
        {
            SourceMember = sourceMember;
            ContextMappingData = contextMappingData;
            Condition = condition;
            IsUseable = isUseable;
        }

        public bool IsUseable { get; }

        public IChildMemberMappingData ContextMappingData { get; }

        public IQualifiedMember SourceMember { get; }

        public Expression Condition { get; }

        public IDataSource CreateDataSource()
        {
            var mapperData = ContextMappingData.MapperData;
            var sourceMember = SourceMember.RelativeTo(mapperData.SourceMember);

            var sourceMemberValue = sourceMember
                .GetQualifiedAccess(mapperData)
                .GetConversionTo(sourceMember.Type);

            var sourceMemberDataSource = new SourceMemberDataSource(
                sourceMember,
                sourceMemberValue,
                Condition,
                mapperData);

            return sourceMemberDataSource;
        }
    }
}