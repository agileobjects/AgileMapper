namespace AgileObjects.AgileMapper.UnitTests.NetCore2
{
    using Common;
    using Microsoft.AspNetCore.JsonPatch;
    using TestClasses;
    using Xunit;

    public class Issue163Tests
    {
        [Fact]
        public void ShouldNotError()
        {
            var source = new JsonPatchDocument<PublicField<int>>();
            var result = Mapper.Map(source).ToANew<JsonPatchDocument<PublicField<int>>>();

            result.ShouldNotBeNull();
        }
    }
}
