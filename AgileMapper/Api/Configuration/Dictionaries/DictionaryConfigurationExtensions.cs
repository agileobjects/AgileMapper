namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;

    internal static class DictionaryConfigurationExtensions
    {
        public static MappingConfigInfo ForDictionary(this MappingConfigInfo configInfo)
            => configInfo.Set(DictionaryType.Dictionary);

        public static MappingConfigInfo ForSourceDictionary<TValue>(this MappingConfigInfo configInfo)
        {
            return configInfo
                .ForDictionary()
                .Set<SourceTypeComparer>(SourceDictionaryTypeComparer<TValue>);
        }

        private static bool SourceDictionaryTypeComparer<TValue>(ITypePair typePair, ITypePair otherTypePair)
        {
            if (!TypePairExtensions.IsForSourceType(typePair, otherTypePair))
            {
                return false;
            }

            if (typeof(TValue) == Constants.AllTypes)
            {
                return true;
            }

            if ((otherTypePair as IQualifiedMemberContext)?.SourceMember is DictionarySourceMember dictionaryMember)
            {
                return typeof(TValue).IsAssignableTo(dictionaryMember.ValueType);
            }

            return false;
        }

        public static MappingConfigInfo ForTargetDictionary(this MappingConfigInfo configInfo)
            => configInfo.ForDictionary().WithMemberTypeComparers();

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
