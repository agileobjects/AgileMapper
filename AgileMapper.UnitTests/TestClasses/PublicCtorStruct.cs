namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal struct PublicCtorStruct<T>
    {
        public PublicCtorStruct(T value)
        {
            Value = value;
        }

        public T Value
        {
            get;
        }
    }
}