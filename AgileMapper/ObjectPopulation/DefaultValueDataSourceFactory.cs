namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new DefaultValueDataSourceFactory();

        public IDataSource Create(MemberMapperData data)
            => new DefaultValueDataSource(data.SourceMember, data.TargetMember.Type);

        private class DefaultValueDataSource : DataSourceBase
        {
            public DefaultValueDataSource(IQualifiedMember member, Type valueType)
                : base(member, Expression.Default(valueType))
            {
            }
        }
    }
}