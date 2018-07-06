namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Collections.Generic;
    using Members.Dictionaries;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DictionaryToDictionaryPopulationLoopData : EnumerableSourcePopulationLoopData
    {
        public DictionaryToDictionaryPopulationLoopData(
            DictionarySourceMember sourceMember,
            ObjectMapperData mapperData)
            : base(
                mapperData.EnumerablePopulationBuilder,
                typeof(KeyValuePair<,>).MakeGenericType(sourceMember.KeyType, sourceMember.ValueType),
                mapperData.SourceObject)
        {
        }

        public override Expression GetSourceElementValue() => Expression.Property(SourceElement, "Value");
    }
}