namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData) => CanMap(mapperData);

        public static bool CanMap(IMemberMapperData mapperData)
        {
            return mapperData.SourceType.IsGenericType() &&
                  (mapperData.SourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) &&
                  DictionaryHasUseableTypes(mapperData);
        }

        private static bool DictionaryHasUseableTypes(IMemberMapperData mapperData)
        {
            var keyAndValueTypes = mapperData.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            var valueType = keyAndValueTypes[1];

            if (mapperData.TargetMember.IsEnumerable)
            {
                return (valueType == typeof(object)) ||
                       (valueType == mapperData.TargetMember.ElementType) ||
                        mapperData.TargetMember.ElementType.IsComplex() ||
                        valueType.IsEnumerable();
            }

            return mapperData
                .MapperContext
                .ValueConverters
                .CanConvert(valueType, mapperData.TargetMember.Type);
        }

        public IEnumerable<IDataSource> Create(IChildMemberMappingData mappingData)
            => DictionaryDataSourceSet.For(mappingData);
    }
}