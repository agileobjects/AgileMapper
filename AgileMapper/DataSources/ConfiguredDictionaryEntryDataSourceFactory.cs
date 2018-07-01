namespace AgileObjects.AgileMapper.DataSources
{
    using Configuration;
    using Members;
    using Members.Dictionaries;

    internal class ConfiguredDictionaryEntryDataSourceFactory : ConfiguredDataSourceFactory
    {
        public ConfiguredDictionaryEntryDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            DictionaryTargetMember targetDictionaryEntryMember)
            : base(configInfo, dataSourceLambda, QualifiedMember.All)
        {
            TargetDictionaryEntryMember = targetDictionaryEntryMember;
        }

        public DictionaryTargetMember TargetDictionaryEntryMember { get; }

        public bool IsFor(IMemberMapperData mapperData) => base.AppliesTo(mapperData);

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (mapperData.TargetMember.IsDictionary)
            {
                return false;
            }

            return mapperData.TargetMember.Matches(TargetDictionaryEntryMember) && 
                   base.AppliesTo(mapperData);
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            return otherItem is ConfiguredDictionaryEntryDataSourceFactory otherDictionaryItem &&
                   TargetDictionaryEntryMember.LeafMember.Equals(otherDictionaryItem.TargetDictionaryEntryMember.LeafMember);
        }
    }
}