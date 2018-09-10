namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicUnconstructable<T>
    {
        protected PublicUnconstructable(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}