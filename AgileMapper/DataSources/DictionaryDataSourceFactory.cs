namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class DictionaryDataSourceFactory : IConditionalDataSourceFactory
    {
        public bool IsFor(MemberMapperData data)
        {
            return data.SourceType.IsGenericType() &&
                  (data.SourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) &&
                  DictionaryHasUseableTypes(data);
        }

        private static bool DictionaryHasUseableTypes(MemberMapperData context)
        {
            var keyAndValueTypes = context.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            if (context.TargetMember.IsEnumerable)
            {
                return (keyAndValueTypes[1] == typeof(object)) ||
                       (keyAndValueTypes[1] == context.TargetMember.ElementType) ||
                        context.TargetMember.ElementType.IsComplex() ||
                        keyAndValueTypes[1].IsEnumerable();
            }

            return context
                .MapperContext
                .ValueConverters
                .CanConvert(keyAndValueTypes[1], context.TargetMember.Type);
        }

        public IDataSource Create(MemberMapperData data) => new DictionaryDataSource(data);
    }
}