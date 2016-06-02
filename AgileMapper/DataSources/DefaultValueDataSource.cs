namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class DefaultValueDataSource : DataSourceBase
    {
        public DefaultValueDataSource(IQualifiedMember member, Type valueType)
            : base(member, Expression.Default(valueType))
        {
        }
    }
}