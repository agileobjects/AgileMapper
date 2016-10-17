namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class DictionaryDataSourceFactory : IConditionalDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData)
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

            if (mapperData.TargetMember.IsEnumerable)
            {
                return (keyAndValueTypes[1] == typeof(object)) ||
                       (keyAndValueTypes[1] == mapperData.TargetMember.ElementType) ||
                        mapperData.TargetMember.ElementType.IsComplex() ||
                        keyAndValueTypes[1].IsEnumerable();
            }

            return mapperData
                .MapperContext
                .ValueConverters
                .CanConvert(keyAndValueTypes[1], mapperData.TargetMember.Type);
        }

        public IDataSource Create(IMemberMapperData mapperData) => new DictionaryDataSource(mapperData);
    }
}