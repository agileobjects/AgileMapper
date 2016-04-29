namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewSimpleTypeMembers
    {
        [Fact]
        public void ShouldCopyAnIntValue()
        {
            var source = new PublicField<int> { Value = 123 };
            var result = Mapper.Map(source).ToNew<PublicProperty<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldCopyAStringValue()
        {
            var source = new PublicProperty<string> { Value = "Oi 'Arry!" };
            var result = Mapper.Map(source).ToNew<PublicField<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(source.Value);
        }
    }
}
