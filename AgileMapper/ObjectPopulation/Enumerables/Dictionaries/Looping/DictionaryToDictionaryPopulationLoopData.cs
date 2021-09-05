namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries.Looping
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Enumerables.Looping;
    using Members.Dictionaries;

    internal class DictionaryToDictionaryPopulationLoopData : EnumerableSourcePopulationLoopData
    {
        private Expression _sourceElement;

        public DictionaryToDictionaryPopulationLoopData(
            DictionarySourceMember sourceMember,
            ObjectMapperData mapperData)
            : base(
                mapperData.EnumerablePopulationBuilder,
                typeof(KeyValuePair<,>).MakeGenericType(sourceMember.KeyType, sourceMember.ValueType),
                mapperData.SourceObject)
        {
        }

        public override Expression SourceElement
            => _sourceElement ??= Expression.Property(base.SourceElement, "Value");
    }
}