namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System.Linq.Expressions;

    internal interface IQueryProviderSettings
    {
        bool SupportsStringEqualsIgnoreCase { get; }

        bool SupportsToString { get; }

        Expression ConvertToString(MethodCallExpression toStringCall);
    }
}