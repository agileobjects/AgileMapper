namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicSetMethod<T>
    {
        public void SetValue(T value)
        {
            Value = value;
        }

        internal T Value
        {
            get;
            private set;
        }
    }
}