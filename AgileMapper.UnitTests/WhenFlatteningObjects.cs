namespace AgileObjects.AgileMapper.UnitTests
{
    using TestClasses;
    using Xunit;
    using Shouldly;

    public class WhenFlatteningObjects
    {
        [Fact]
        public void ShouldIncludeASimpleTypeProperty()
        {
            var source = new PublicProperty<string> { Value = "Flatten THIS" };
            var result = Mapper.Flatten(source);

            ((object)result).ShouldNotBeNull();
            ((string)result.Value).ShouldBe("Flatten THIS");
        }
    }
}
