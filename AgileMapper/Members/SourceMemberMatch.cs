namespace AgileObjects.AgileMapper.Members
{
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
            bool isUseable = true)
        {
            SourceMember = GetFinalSourceMember(sourceMember, contextMappingData.MapperData);
            ContextMappingData = contextMappingData;
            IsUseable = isUseable;
        }

        private static IQualifiedMember GetFinalSourceMember(
            IQualifiedMember sourceMember,
            IMemberMapperData targetMapperData)
        {
            return targetMapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, targetMapperData.TargetMember);
        }

        public IQualifiedMember SourceMember { get; }

        public IChildMemberMappingData ContextMappingData { get; }

        public bool IsUseable { get; }

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
                mapperData);

            return sourceMemberDataSource;
        }
    }
}