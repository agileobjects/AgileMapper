namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using Members;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        private readonly MapperContext _mapperContext;

        public DictionaryDataSourceFactory(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public bool IsFor(IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && mapperData.Context.IsStandalone)
            {
                return false;
            }

            return HasUseableSourceDictionary(mapperData);
        }

        private bool HasUseableSourceDictionary(IMemberMapperData mapperData)
        {
            DictionarySourceMember dictionarySourceMember;

            if (!mapperData.SourceMemberIsStringKeyedDictionary(out dictionarySourceMember))
            {
                return false;
            }

            if (dictionarySourceMember.HasObjectEntries)
            {
                return true;
            }

            var valueType = dictionarySourceMember.ValueType;

            Type targetType;

            if (mapperData.TargetMember.IsEnumerable)
            {
                if (valueType.IsEnumerable())
                {
                    return true;
                }

                targetType = mapperData.TargetMember.ElementType;

                if ((valueType == targetType) || targetType.IsComplex())
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

        public IEnumerable<IDataSource> Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsSimple)
            {
                yield return new DictionaryEntryDataSource(new DictionaryEntryVariablePair(mapperData));
                yield break;
            }

            var sourceMember = GetSourceMember(mapperData);

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

        private class DictionaryNonSimpleMemberDataSource : DataSourceBase
        {
            public DictionaryNonSimpleMemberDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
                : base(
                    sourceMember,
                    sourceMember.GetQualifiedAccess(mapperData.SourceObject))
            {
            }
        }
    }
}