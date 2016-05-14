namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class NullValueProviderFactory : IValueProviderFactory
    {
        public static readonly IValueProviderFactory Instance = new NullValueProviderFactory();

        public ValueProvider Create(IMemberMappingContext context)
            => ValueProvider.Null(ctx => Constants.EmptyExpression);
    }
}