namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using Extensions;
    using Members;

    internal class DictionaryDataSourceFactory : IConditionalDataSourceFactory
    {
        public bool IsFor(IMemberMappingContext context)
        {
            return context.SourceType.IsGenericType &&
                  (context.SourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) &&
                  DictionaryHasUseableTypes(context);
        }

        private static bool DictionaryHasUseableTypes(IMemberMappingContext context)
        {
            var keyAndValueTypes = context.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            if (context.TargetMember.IsEnumerable)
            {
                return (keyAndValueTypes[1] == typeof(object)) ||
                       (keyAndValueTypes[1] == context.TargetMember.LeafMember.ElementType) ||
                        context.TargetMember.LeafMember.ElementType.IsComplex() ||
                        keyAndValueTypes[1].IsEnumerable();
            }

            return context
                .MapperContext
                .ValueConverters
                .CanConvert(keyAndValueTypes[1], context.TargetMember.Type);
        }

        public IDataSource Create(IMemberMappingContext context) => new DictionaryDataSource(context);
    }
}