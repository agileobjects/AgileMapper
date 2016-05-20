namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;

    internal interface IConfiguredDataSource : IDataSource
    {
        bool IsConditional { get; }

        Expression OriginalValue { get; }
    }
}