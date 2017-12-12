namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using Shouldly;
    using Xunit;

    public class WhenMappingToNewDynamics
    {
        [Fact]
        public void ShouldMapToASimpleTypeMember()
        {
            var result = Mapper.Map(new { Value = 123 }).ToANew<dynamic>();

            ((object)result).ShouldNotBeNull();
            ((int)result.Value).ShouldBe(123);
        }
    }
}
