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
        public bool IsFor(MemberMapperData mapperData)
        {
            return mapperData.SourceType.IsGenericType() &&
                  (mapperData.SourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) &&
                  DictionaryHasUseableTypes(mapperData);
        }

        private static bool DictionaryHasUseableTypes(MemberMapperData mapperData)
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

        public IDataSource Create(MemberMapperData mapperData) => new DictionaryDataSource(mapperData);
    }
}