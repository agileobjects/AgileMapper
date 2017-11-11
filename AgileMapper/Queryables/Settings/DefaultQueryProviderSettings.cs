namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System.Linq.Expressions;

    internal class DefaultQueryProviderSettings : IQueryProviderSettings
    {
        public virtual bool SupportsStringEqualsIgnoreCase => false;

        public virtual bool SupportsToString => false;

        public virtual Expression ConvertToString(MethodCallExpression toStringCall) => toStringCall;
    }
}