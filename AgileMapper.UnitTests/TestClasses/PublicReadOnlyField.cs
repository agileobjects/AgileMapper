namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicReadOnlyField<T>
    {
        public readonly T Value;

        public PublicReadOnlyField(T value)
        {
            Value = value;
        }
    }
}