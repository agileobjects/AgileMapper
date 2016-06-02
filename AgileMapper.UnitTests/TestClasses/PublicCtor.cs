namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicCtor<T>
    {
        public PublicCtor(T value)
        {
            Value = value;
        }

        public T Value
        {
            get;
        }
    }
}