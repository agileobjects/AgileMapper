namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData)
        {
            if (mapperData.HasUseableSourceDictionary())
            {
                if (mapperData.TargetMember.IsComplex)
                {
                    return !mapperData.IsRoot;
                }

                return true;
            }

            return false;
        }

        public IEnumerable<IMaptimeDataSource> Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceMember = GetSourceMember(mapperData);

            if (mapperData.TargetMember.IsSimple)
            {
                yield return new DictionaryEntryDataSource(sourceMember, mapperData);
                yield break;
            }

            if (UseComplexTypeDataSource(sourceMember, mapperData))
            {
                yield return new DictionaryComplexTypeMemberDataSource(sourceMember, mappingData);
            }

            yield return new DictionaryNonSimpleMemberDataSource(sourceMember, mapperData);
        }

        private static DictionarySourceMember GetSourceMember(IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsRecursive)
            {
                return new DictionarySourceMember(mapperData);
            }

            var parentMapperData = mapperData.Parent;

            while (!parentMapperData.IsRoot)
            {
                if (parentMapperData.TargetMember.LeafMember == mapperData.TargetMember.LeafMember)
                {
                    break;
                }

                parentMapperData = parentMapperData.Parent;
            }

            return (DictionarySourceMember)parentMapperData.SourceMember;
        }

        private static bool UseComplexTypeDataSource(DictionarySourceMember sourceMember, IBasicMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsComplex)
            {
                return false;
            }

            if (sourceMember.EntryType == typeof(object))
            {
                return true;
            }

            return sourceMember.EntryType.IsAssignableFrom(mapperData.TargetMember.Type) ||
                   mapperData.TargetMember.Type.IsAssignableFrom(sourceMember.EntryType);
        }

        private class DictionaryNonSimpleMemberDataSource : DataSourceBase, IMaptimeDataSource
        {
            public DictionaryNonSimpleMemberDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
                : base(
                      sourceMember,
                      sourceMember.GetQualifiedAccess(mapperData.SourceObject))
            {
            }

            public bool WrapInFinalDataSource => true;
        }
    }
}