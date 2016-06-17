namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class PreserveExistingValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new PreserveExistingValueDataSourceFactory();

        public IDataSource Create(IMemberMappingContext context) => new PreserveExistingValueDataSource(context);

        private class PreserveExistingValueDataSource : DataSourceBase
        {
            public PreserveExistingValueDataSource(IMemberMappingContext context)
                : base(
                      context.SourceMember,
                      context.TargetMember.IsReadable
                          ? context.TargetMember.GetAccess(context.InstanceVariable)
                          : Constants.EmptyExpression,
                      context)
            {
            }

            public override Expression GetValueOption(Expression valueSoFar)
                => Expression.Condition(Value.GetIsNotDefaultComparison(), Value, valueSoFar);
        }
    }
}