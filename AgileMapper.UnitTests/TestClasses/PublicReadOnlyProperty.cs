namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicReadOnlyProperty<T>
    {
        public PublicReadOnlyProperty(T readOnlyValue)
        {
            Value = readOnlyValue;
        }

        public T Value { get; }
    }

    internal static class ReadOnlyPropertyExtensions
    {
        public static void CreateAReadOnlyPropertyUsing<T>(this IMapper mapper, T value)
        {
            mapper.WhenMapping
                .To<PublicReadOnlyProperty<T>>()
                .CreateInstancesUsing(data => new PublicReadOnlyProperty<T>(value));
        }
    }
}