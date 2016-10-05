namespace AgileObjects.AgileMapper.UnitTests.Polyfills
{
    using TestClasses;
    using Shouldly;
    using Xunit;

    public class WhenMappingFieldsAndProperties
    {
        [Fact]
        public void ShouldMapFromAFieldToAProperty()
        {
            var source = new PublicField<string> { Value = "Hello!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Hello!");
        }

        [Fact]
        public void ShouldMapFromAPropertyToAField()
        {
            var source = new PublicProperty<string> { Value = "Goodbye!" };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Goodbye!");
        }
    }
}
