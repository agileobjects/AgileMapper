namespace AgileObjects.AgileMapper.DataSources
{
    using Configuration;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredDictionaryDataSourceFactory : ConfiguredDataSourceFactory
    {
        public ConfiguredDictionaryDataSourceFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo dataSourceLambda,
            DictionaryTargetMember targetDictionaryEntryMember)
            : base(configInfo, dataSourceLambda, QualifiedMember.All)
        {
            TargetDictionaryEntryMember = targetDictionaryEntryMember;
        }

        public DictionaryTargetMember TargetDictionaryEntryMember { get; }

        public bool IsFor(ObjectMapperData mapperData) => base.AppliesTo(mapperData);

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            if (mapperData.TargetMember.IsDictionary)
            {
                return false;
            }

            return Matches(mapperData.TargetMember) && base.AppliesTo(mapperData);
        }

        public bool Matches(QualifiedMember targetMember)
            => targetMember.Matches(TargetDictionaryEntryMember);
    }
}