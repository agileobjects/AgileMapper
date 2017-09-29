namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewStructs
    {
        [Fact]
        public void ShouldMapFromAnAnonymousTypeToAStruct()
        {
            var source = new { Value = "Hello struct!" };
            var result = Mapper.Map(source).ToANew<PublicPropertyStruct<string>>();

            result.Value.ShouldBe(source.Value);
        }
    }
}
