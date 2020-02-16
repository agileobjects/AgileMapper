namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    public interface IPublicInterface<T>
    {
        T Value { get; set; }
    }

    public interface IPublicInterface
    {
        object Value { get; set; }
    }
}