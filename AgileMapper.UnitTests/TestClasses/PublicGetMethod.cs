namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicGetMethod<T>
    {
        private readonly T _value;

        public PublicGetMethod(T value)
        {
            _value = value;
        }

        public T GetValue()
        {
            return _value;
        }
    }
}