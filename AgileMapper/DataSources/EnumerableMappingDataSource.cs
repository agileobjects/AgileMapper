namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class EnumerableMappingDataSource : IDataSource
    {
        private readonly IDataSource _sourceEnumerableDataSource;
        private readonly Member _enumerableMember;

        public EnumerableMappingDataSource(IDataSource sourceEnumerableDataSource, Member enumerableMember)
        {
            _sourceEnumerableDataSource = sourceEnumerableDataSource;
            _enumerableMember = enumerableMember;
        }

        public Expression GetValue(IObjectMappingContext omc)
        {
            var sourceEnumerable = _sourceEnumerableDataSource.GetValue(omc);

            return omc.GetMapCall(sourceEnumerable, _enumerableMember);
        }
    }
}