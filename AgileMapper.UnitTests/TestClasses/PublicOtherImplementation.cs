namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    public class PublicOtherImplementation<T> : IPublicInterface<T>, IPublicInterface
    {
        public T Value { get; set; }
        
        object IPublicInterface.Value { get => Value; set => Value = (T)value; }
    }
}