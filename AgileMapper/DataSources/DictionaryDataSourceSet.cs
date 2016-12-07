namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;

    internal static class DictionaryDataSourceSet
    {
        public static IEnumerable<IDataSource> For(IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var sourceMember = new DictionarySourceMember(childMapperData);

            if ((sourceMember.EntryType == typeof(object)) ||
                (childMapperData.TargetMember.IsEnumerable == sourceMember.EntryType.IsEnumerable()))
            {
                yield return new DictionaryEntryDataSource(sourceMember, childMappingData);
            }

            yield return GetFallbackDataSource(sourceMember, childMappingData);
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
                .Create(childMappingData.MapperData);
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
    }
}