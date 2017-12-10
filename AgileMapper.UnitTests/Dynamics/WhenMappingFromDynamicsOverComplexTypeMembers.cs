namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsOverComplexTypeMembers
    {
        [Fact]
        public void ShouldMapFromANestedDynamicToANestedComplexType()
        {
            dynamic sourceDynamic = new ExpandoObject();

            sourceDynamic.Line1 = "Over there";

            var source = new PublicTwoFields<dynamic, string>
            {
                Value1 = sourceDynamic,
                Value2 = "Good Googley Moogley!"
            };

            var target = new PublicTwoFields<Address, string>
            {
                Value1 = new Address { Line1 = "Over here", Line2 = "Somewhere else" },
                Value2 = "Nothing"
            };

            var preMappingAddress = target.Value1;

            Mapper.Map(source).Over(target);

            target.Value1.ShouldBeSameAs(preMappingAddress);
            target.Value1.Line1.ShouldBe("Over there");
            target.Value1.Line2.ShouldBe("Somewhere else");
            target.Value2.ShouldBe("Good Googley Moogley!");
        }
    }
}
