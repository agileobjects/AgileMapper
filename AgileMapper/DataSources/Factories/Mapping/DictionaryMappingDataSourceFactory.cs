namespace AgileObjects.AgileMapper.DataSources.Factories.Mapping
{
    using Members.Dictionaries;
    using ObjectPopulation;

    internal class DictionaryMappingDataSourceFactory : MappingDataSourceFactoryBase
    {
        public DictionaryMappingDataSourceFactory()
            : base(new DictionaryMappingExpressionFactory())
        {
        }

        public override bool IsFor(IObjectMappingData mappingData)
        {
            if (mappingData.MapperData.TargetMember.IsDictionary)
            {
                return true;
            }

            if (mappingData.IsRoot)
            {
                return false;
            }

            if (!(mappingData.MapperData.TargetMember is DictionaryTargetMember dictionaryMember))
            {
                return false;
            }

            if (dictionaryMember.HasSimpleEntries)
            {
                return true;
            }

            return dictionaryMember.HasObjectEntries && !mappingData.IsStandalone();
        }
    }
}