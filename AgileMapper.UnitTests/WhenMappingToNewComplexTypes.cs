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
        public void ShouldCreateAResultObjectViaAParameterisedConstructor()
        {
            var source = new PublicGetMethod<string>("Barney");
            var result = Mapper.Map(source).ToNew<PublicCtor<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Barney");
        }

        [Fact]
        public void ShouldConvertASimpleTypeConstructorArgument()
        {
            var source = new PublicGetMethod<string>("80.6537");
            var result = Mapper.Map(source).ToNew<PublicCtor<decimal>>();

            result.Value.ShouldBe(80.6537);
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

        [Fact]
        public void ShouldCopyAnIntValue()
        {
            var source = new PublicField<int> { Value = 123 };
            var result = Mapper.Map(source).ToNew<PublicProperty<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldCopyAStringValue()
        {
            var source = new PublicProperty<string> { Value = "Oi 'Arry!" };
            var result = Mapper.Map(source).ToNew<PublicField<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Oi 'Arry!");
        }
    }
}
