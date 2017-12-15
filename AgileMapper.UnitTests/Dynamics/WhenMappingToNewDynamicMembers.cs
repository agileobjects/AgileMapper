namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewDynamicMembers
    {
        [Fact]
        public void ShouldMapFromAFlattenedMember()
        {
            var source = new
            {
                ValueLine1 = "Over here!",
                Value = new { Line2 = "Over there!" },
                Va = new { Lu = new { E = new { Line3 = "Over where?!" } } }
            };

            var result = Mapper.Map(source).ToANew<PublicField<ExpandoObject>>();

            ((object)result.Value).ShouldNotBeNull();
            dynamic resultDynamic = result.Value;
            ((string)resultDynamic.Line1).ShouldBe("Over here!");
            ((string)resultDynamic.Line2).ShouldBe("Over there!");
            ((string)resultDynamic.Line3).ShouldBe("Over where?!");
        }
    }
}
