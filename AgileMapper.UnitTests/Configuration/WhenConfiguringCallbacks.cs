namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringCallbacks
    {
        [Fact]
        public void ShouldCallAnObjectCreatedCallback()
        {
            using (var mapper = Mapper.Create())
            {
                var createdInstance = default(PublicProperty<int>);

                mapper.When
                    .CreatingInstances
                    .Call(instance => createdInstance = (PublicProperty<int>)instance);

                var source = new PublicField<int>();
                var result = mapper.Map(source).ToNew<PublicProperty<int>>();

                createdInstance.ShouldNotBeNull();
                result.ShouldBe(createdInstance);
            }
        }

        [Fact]
        public void ShouldCallAnObjectCreatedCallbackForASpecifiedType()
        {
            using (var mapper = Mapper.Create())
            {
                var createdInstance = default(Person);

                mapper.When
                    .CreatingInstances
                    .Of<Person>()
                    .Call(instance => createdInstance = instance);

                var source = new Person { Name = "Alex" };
                var result = mapper.Map(source).ToNew<Person>();

                createdInstance.ShouldNotBeNull();
                result.ShouldBe(createdInstance);
            }
        }

        [Fact]
        public void ShouldRestrictAnObjectCreatedCallbackToASpecifiedType()
        {
            using (var mapper = Mapper.Create())
            {
                var createdInstance = default(Person);

                mapper.When
                    .CreatingInstances
                    .Of<Person>()
                    .Call(instance => createdInstance = instance);

                var source = new { Value = "12345" };
                var result = mapper.Map(source).ToNew<PublicProperty<int>>();

                result.Value.ShouldBe(12345);
                createdInstance.ShouldBeNull();
            }
        }
    }
}
