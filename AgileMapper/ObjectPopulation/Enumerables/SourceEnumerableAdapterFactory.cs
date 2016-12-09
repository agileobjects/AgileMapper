namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using Members;

    internal static class SourceEnumerableAdapterFactory
    {
        public static ISourceEnumerableAdapter GetAdapterFor(EnumerablePopulationBuilder builder)
        {
            if (builder.MapperData.HasSourceDictionary())
            {
                var sourceMember = new DictionarySourceMember(builder.MapperData);

                if (sourceMember.HasObjectEntries)
                {
                    return new SourceObjectDictionaryAdapter(sourceMember, builder);
                }

                if (sourceMember.CouldContainSourceInstance)
                {
                    return new SourceInstanceDictionaryAdapter(sourceMember, builder);
                }

                return new SourceElementsDictionaryAdapter(builder);
            }

            return new DefaultSourceEnumerableAdapter(builder);
        }
    }
}