namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections;
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;

    internal class DictionaryDataSourceSet : IEnumerable<IDataSource>
    {
        private readonly IEnumerable<IDataSource> _dataSources;

        private DictionaryDataSourceSet(params IDataSource[] dataSources)
        {
            _dataSources = dataSources;
        }

        public static DictionaryDataSourceSet For(IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var sourceMember = new DictionarySourceMember(childMapperData);

            var fallbackDataSource = GetFallbackDataSource(sourceMember, childMappingData);

            if ((sourceMember.EntryType != typeof(object)) &&
                (childMapperData.TargetMember.IsEnumerable != sourceMember.EntryType.IsEnumerable()))
            {
                return new DictionaryDataSourceSet(fallbackDataSource);
            }

            var dictionaryEntryDataSource =
                new DictionaryEntryDataSource(sourceMember, childMappingData);

            return new DictionaryDataSourceSet(dictionaryEntryDataSource, fallbackDataSource);
        }

        private static IDataSource GetFallbackDataSource(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
        {
            if (DictionaryEntriesCouldBeEnumerableElements(sourceMember, childMappingData))
            {
                return new DictionaryToEnumerableDataSource(sourceMember, childMappingData);
            }

            return childMappingData
                .RuleSet
                .FallbackDataSourceFactory
                .Create(childMappingData);
        }

        private static bool DictionaryEntriesCouldBeEnumerableElements(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
        {
            if (!childMappingData.MapperData.TargetMember.IsEnumerable)
            {
                return false;
            }

            if (sourceMember.EntryType == typeof(object))
            {
                return true;
            }

            var targetElementsAreCompatibleWithEntries = childMappingData.MapperData
                .TargetMember.ElementType
                .IsAssignableFrom(sourceMember.EntryType);

            return targetElementsAreCompatibleWithEntries;
        }

        public IEnumerator<IDataSource> GetEnumerator() => _dataSources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}