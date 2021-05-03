namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using Configuration;
    using DataSources;
    using Lambdas;
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

        public override bool AppliesTo(IQualifiedMemberContext context)
        {
            if (context.TargetMember.IsDictionary)
            {
                return false;
            }

            return context.TargetMember.Matches(TargetDictionaryEntryMember) &&
                   base.AppliesTo(context);
        }

        protected override bool MembersConflict(UserConfiguredItemBase otherItem)
        {
            return otherItem is ConfiguredDictionaryEntryDataSourceFactory otherDictionaryItem &&
                   TargetDictionaryEntryMember.LeafMember.Equals(otherDictionaryItem.TargetDictionaryEntryMember.LeafMember);
        }
    }
}