namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;

    public class PublicImplementation<T> : IPublicInterface<T>, IPublicInterface
    {
        public T Value { get; set; }

        object IPublicInterface.Value
        {
            get => Value;
            set => Value = (T)Convert.ChangeType(value, typeof(T));
        }
    }
}