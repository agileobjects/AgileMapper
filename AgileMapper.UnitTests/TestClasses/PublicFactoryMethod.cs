namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicFactoryMethod<T>
    {
        private PublicFactoryMethod(T value)
        {
            Value = value;
        }

        public static PublicFactoryMethod<T> Create(T value) 
            => new PublicFactoryMethod<T>(value);

        public T Value { get; }
    }
}