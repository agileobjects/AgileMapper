namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class DefaultValueDataSource : DataSourceBase
    {
        public DefaultValueDataSource(IQualifiedMember sourceMember, Type valueType)
            : base(sourceMember, Expression.Default(valueType))
        {
        }
    }
}