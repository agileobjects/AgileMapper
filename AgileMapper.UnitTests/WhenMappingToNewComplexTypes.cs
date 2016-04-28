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
            using (IMapper mapper = new Mapper())
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
