namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using Members;

    internal static class SourceEnumerableAdapterFactory
    {
        public static ISourceEnumerableAdapter GetAdapterFor(EnumerablePopulationBuilder builder)
        {
            var dictionarySourceMember = builder.MapperData.SourceMember as DictionarySourceMember;

            if ((dictionarySourceMember != null) || builder.MapperData.HasSourceDictionary())
            {
                if (dictionarySourceMember == null)
                {
                    dictionarySourceMember = new DictionarySourceMember(builder.MapperData);
                }

                if (dictionarySourceMember.HasObjectEntries)
                {
                    return new SourceObjectDictionaryAdapter(dictionarySourceMember, builder);
                }

                if (dictionarySourceMember.CouldContainSourceInstance)
                {
                    return new SourceInstanceDictionaryAdapter(dictionarySourceMember, builder);
                }

                return new SourceElementsDictionaryAdapter(builder);
            }

            return new DefaultSourceEnumerableAdapter(builder);
        }
    }
}