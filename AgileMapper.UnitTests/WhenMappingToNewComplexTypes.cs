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

        [Fact]
        public void ShouldMapUsingStaticCloneMethod()
        {
            var source = new Person { Name = "Barney" };
            var result = Mapper.Clone(source);

            result.ShouldNotBeSameAs(source);
            result.Name.ShouldBe("Barney");
        }

        [Fact]
        public void ShouldMapUsingInstanceCloneMethod()
        {
            var source = new Person { Name = "Maggie" };
            var result = Mapper.Create().Clone(source);

            result.ShouldNotBeSameAs(source);
            result.Name.ShouldBe("Maggie");
        }
    }
}
