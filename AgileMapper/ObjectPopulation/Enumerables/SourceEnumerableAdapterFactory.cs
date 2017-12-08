namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using Dictionaries;
    using Members;

    internal static class SourceEnumerableAdapterFactory
    {
        public static ISourceEnumerableAdapter GetAdapterFor(EnumerablePopulationBuilder builder)
        {
            var dictionarySourceMember = builder.MapperData.GetDictionarySourceMemberOrNull();

            if (dictionarySourceMember != null)
            {
                if (!builder.MapperData.IsRoot)
                {
                    if (dictionarySourceMember.HasObjectEntries)
                    {
                        return new SourceObjectDictionaryAdapter(dictionarySourceMember, builder);
                    }

                    if (dictionarySourceMember.CouldContainSourceInstance)
                    {
                        return new SourceInstanceDictionaryAdapter(dictionarySourceMember, builder);
                    }
                }

                return new SourceElementsDictionaryAdapter(dictionarySourceMember, builder);
            }

            return new DefaultSourceEnumerableAdapter(builder);
        }
    }
}