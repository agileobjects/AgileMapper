namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class PublicReadOnlyProperty<T>
    {
        public T Value
        {
            get;
            internal set;
        }
    }
}