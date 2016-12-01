namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicReadOnlyField<T>
    {
        public readonly T Value;

        public PublicReadOnlyField(T readOnlyValue)
        {
            Value = readOnlyValue;
        }
    }

    internal static class ReadOnlyFieldExtensions
    {
        public static void CreateAReadOnlyFieldUsing<T>(this IMapper mapper, T value)
        {
            mapper.WhenMapping
                .To<PublicReadOnlyField<T>>()
                .CreateInstancesUsing(data => new PublicReadOnlyField<T>(value));
        }
    }
}