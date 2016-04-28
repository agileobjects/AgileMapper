namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IDataSource
    {
        Expression GetValue(IObjectMappingContext omc);
    }
}
