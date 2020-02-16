namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using AgileObjects.AgileMapper.Members;

    internal static class DictionaryConfigurationExtensions
    {
        public static MappingConfigInfo ForTargetDictionary(this MappingConfigInfo configInfo)
            => configInfo.Set(DictionaryType.Dictionary).WithMemberTypeComparers();

        public static MappingConfigInfo WithMemberTypeComparers(this MappingConfigInfo configInfo)
        {
            return configInfo
                .Set<SourceTypeComparer>(SourceMemberTypeComparer)
                .Set<TargetTypeComparer>(TargetMemberTypeComparer);
        }

        private static bool SourceMemberTypeComparer(ITypePair typePair, ITypePair otherTypePair)
        {
            if (TypePairExtensions.IsForSourceType(typePair, otherTypePair))
            {
                return true;
            }

            return (otherTypePair is IQualifiedMemberContext context) &&
                   (context.SourceMember?.HasCompatibleType(typePair.SourceType) == true);
        }

        private static bool TargetMemberTypeComparer(ITypePair typePair, ITypePair otherTypePair)
        {
            if (TypePairExtensions.IsForTargetType(typePair, otherTypePair))
            {
                return true;
            }

            return (otherTypePair is IQualifiedMemberContext context) &&
                   (context.TargetMember?.HasCompatibleType(typePair.TargetType) == true);
        }
    }
}
