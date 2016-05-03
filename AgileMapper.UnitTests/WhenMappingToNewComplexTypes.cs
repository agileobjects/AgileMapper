namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewComplexTypes
    {
        [Fact]
        public void ShouldCreateAResultObjectViaADefaultConstructor()
        {
            var source = new PublicField<string>();
            var result = Mapper.Map(source).ToNew<PublicProperty<string>>();

            result.ShouldNotBeNull();
        }

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

        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Value = "Hello there!" };
            var result = Mapper.Map(source).ToNew<PublicProperty<string>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var result = Mapper.Map(default(PublicProperty<int>)).ToNew<PublicField<int>>();

            result.ShouldBeNull();
        }
    }
}
