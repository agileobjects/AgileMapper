namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context)
            => context.TargetMember.IsReadable
                ? new ExistingMemberValueOrEmptyDataSource(context)
                : DefaultValueDataSourceFactory.Instance.Create(context);

        private class ExistingMemberValueOrEmptyDataSource : DataSourceBase
        {
            public ExistingMemberValueOrEmptyDataSource(IMemberMappingContext context)
                : base(context.SourceMember, GetValue(context), context)
            {
            }

            private static Expression GetValue(IMemberMappingContext context)
            {
                var existingValue = context.TargetMember.GetAccess(context.InstanceVariable);

                if (!context.TargetMember.IsEnumerable)
                {
                    return existingValue;
                }

                var emptyEnumerable = EnumerableTypes.GetEnumerableEmptyInstance(context);

                return Expression.Coalesce(existingValue, emptyEnumerable);
            }
        }
    }
}