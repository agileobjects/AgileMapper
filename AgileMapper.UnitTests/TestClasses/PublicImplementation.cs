namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    public class PublicImplementation<T> : IPublicInterface<T>
    {
        public T Value { get; set; }
    }
}