namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicWriteOnlyProperty<T>
    {
        public T Value
        {
            internal get;
            set;
        }
    }
}