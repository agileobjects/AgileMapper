namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using Xunit;

    public class WhenMappingToNewDynamics
    {
        [Fact]
        public void ShouldMapToADynamicSimpleTypeMember()
        {
            var result = Mapper.Map(new { Value = 123 }).ToANew<dynamic>();

            ((object)result).ShouldNotBeNull();
            ((int)result.Value).ShouldBe(123);
        }

        [Fact]
        public void ShouldMapToAnExpandoObjectSimpleTypeMember()
        {
            dynamic result = Mapper.Map(new { Value = "Oh so dynamic" }).ToANew<ExpandoObject>();

            ((object)result).ShouldNotBeNull();
            ((string)result.Value).ShouldBe("Oh so dynamic");
        }
    }
}
