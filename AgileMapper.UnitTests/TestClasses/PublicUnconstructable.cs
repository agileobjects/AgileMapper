namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicUnconstructable<T>
    {
        protected PublicUnconstructable(T value)
        {
            Value = value;
        }

        internal static PublicUnconstructable<T> MakeOne(T unconstructableValue)
            => new PublicUnconstructable<T>(unconstructableValue);

        public T Value { get; }
    }
}