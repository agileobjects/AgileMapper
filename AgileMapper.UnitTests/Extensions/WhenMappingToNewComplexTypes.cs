namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewComplexTypes
    {
        [Fact]
        public void ShouldCopyAnIntValueInADeepClone()
        {
            var source = new PublicProperty<int> { Value = 123 };
            var result = source.DeepClone();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }
    }
}
