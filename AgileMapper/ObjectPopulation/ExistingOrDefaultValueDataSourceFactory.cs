namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(MemberMapperData data)
            => data.TargetMember.IsReadable
                ? new ExistingMemberValueOrEmptyDataSource(data)
                : DefaultValueDataSourceFactory.Instance.Create(data);

        private class ExistingMemberValueOrEmptyDataSource : DataSourceBase
        {
            public ExistingMemberValueOrEmptyDataSource(MemberMapperData data)
                : base(data.SourceMember, GetValue(data), data)
            {
            }

            private static Expression GetValue(MemberMapperData data)
            {
                var existingValue = data.TargetMember.GetAccess(data.InstanceVariable);

                if (!data.TargetMember.IsEnumerable)
                {
                    return existingValue;
                }

                var emptyEnumerable = data.TargetMember.GetEmptyInstanceCreation();

                return Expression.Coalesce(existingValue, emptyEnumerable);
            }
        }
    }
}