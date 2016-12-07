namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class DictionaryEntryVariablePair
    {
        public DictionaryEntryVariablePair(DictionarySourceMember sourceMember, IBasicMapperData childMapperData)
        {
            Key = Expression.Variable(typeof(string), childMapperData.TargetMember.Name.ToCamelCase() + "Key");

            var valueVariableName = childMapperData.TargetMember.Name.ToCamelCase();
            Value = Expression.Variable(sourceMember.EntryType, valueVariableName);
        }

        public ParameterExpression Key { get; }

        public ParameterExpression Value { get; }
    }
}