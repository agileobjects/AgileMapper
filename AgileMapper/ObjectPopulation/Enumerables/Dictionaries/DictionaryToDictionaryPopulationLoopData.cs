namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

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

        protected override Expression GetSourceElementValue() => Expression.Property(SourceElement, "Value");
    }
}