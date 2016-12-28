namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        private readonly MapperContext _mapperContext;

        public DictionaryDataSourceFactory(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public bool IsFor(IBasicMapperData mapperData)
        {
            if (HasUseableSourceDictionary(mapperData))
            {
                if (mapperData.TargetMember.IsComplex)
                {
                    return !mapperData.IsRoot;
                }

                return true;
            }

            return false;
        }

        private bool HasUseableSourceDictionary(IBasicMapperData mapperData)
        {
            if (!mapperData.SourceType.IsDictionary())
            {
                return false;
            }

            var keyAndValueTypes = mapperData.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            var valueType = keyAndValueTypes[1];
            Type targetType;

            if (mapperData.TargetMember.IsEnumerable)
            {
                targetType = mapperData.TargetMember.ElementType;

                if ((valueType == typeof(object)) || (valueType == targetType) ||
                    targetType.IsComplex() || valueType.IsEnumerable())
                {
                    return true;
                }
            }
            else
            {
                targetType = mapperData.TargetMember.Type;
            }

            return _mapperContext.ValueConverters.CanConvert(valueType, targetType);
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