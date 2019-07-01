namespace AgileObjects.AgileMapper.Members
{
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
            SourceMember = GetFinalSourceMember(sourceMember, contextMappingData);
            ContextMappingData = contextMappingData;
            IsUseable = isUseable;
        }

        private static IQualifiedMember GetFinalSourceMember(
            IQualifiedMember sourceMember,
            IChildMemberMappingData targetData)
        {
            return targetData
                .MapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, targetData.MapperData.TargetMember);
        }

        public IQualifiedMember SourceMember { get; }

        public IChildMemberMappingData ContextMappingData { get; }

        public bool IsUseable { get; }
    }
}