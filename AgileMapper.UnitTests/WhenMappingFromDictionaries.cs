namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDictionaries
    {
        [Fact]
        public void ShouldPopulateASimpleTypeMember()
        {
            var source = new Dictionary<string, int> { ["Value"] = 123 };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }
    }
}
