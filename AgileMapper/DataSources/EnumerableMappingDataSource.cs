namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class EnumerableMappingDataSource : IDataSource
    {
        private readonly IDataSource _sourceEnumerableDataSource;

        public EnumerableMappingDataSource(
            IDataSource sourceEnumerableDataSource,
            Member enumerableMember,
            IObjectMappingContext omc)
        {
            _sourceEnumerableDataSource = sourceEnumerableDataSource;
            Value = omc.GetMapCall(sourceEnumerableDataSource.Value, enumerableMember);
        }

        public Expression GetConditionOrNull(ParameterExpression contextParameter) => null;

        public IEnumerable<Expression> NestedSourceMemberAccesses
            => _sourceEnumerableDataSource.NestedSourceMemberAccesses;

        public Expression Value { get; }
    }
}