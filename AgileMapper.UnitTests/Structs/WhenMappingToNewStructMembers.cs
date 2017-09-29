namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewStructMembers
    {
        [Fact]
        public void ShouldMapToANestedConstructorlessStruct()
        {
            var source = new { Value = new { Value1 = "Hello", Value2 = "Goodbye" } };

            var result = Mapper
                .Map(source)
                .ToANew<PublicField<PublicTwoFieldsStruct<string, string>>>();

            result.Value.Value1.ShouldBe("Hello");
            result.Value.Value2.ShouldBe("Goodbye");
        }

        [Fact]
        public void ShouldMapToANestedStructConstructor()
        {
            var source = new { Value = new { Value = "800" } };
            var result = Mapper.Map(source).ToANew<PublicPropertyStruct<PublicCtorStruct<int>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldNotBeNull();
            result.Value.Value.ShouldBe(800);
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceForNestedCtorParameter()
        {
            var source = new { Value = new { Hello = "123" } };
            var result = Mapper.Map(source).ToANew<PublicField<PublicCtorStruct<short>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeDefault();
            result.Value.Value.ShouldBeDefault();
        }
    }
}
