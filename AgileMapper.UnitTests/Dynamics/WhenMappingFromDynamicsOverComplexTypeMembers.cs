namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
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

        [Fact]
        public void ShouldMapFlattenedMembersFromANestedDynamicToANestedComplexType()
        {
            dynamic sourceDynamic = new ExpandoObject();

            sourceDynamic.Name = "Mystery :o";
            sourceDynamic.AddressLine1 = "Over here";
            sourceDynamic.AddressLine2 = "Over there";

            var source = new PublicTwoFields<dynamic, string>
            {
                Value1 = sourceDynamic,
                Value2 = "Blimey!!"
            };

            var target = new PublicTwoFields<MysteryCustomer, string>
            {
                Value1 = new MysteryCustomer { Name = "Mystery?!" },
                Value2 = "Nowt"
            };

            var preMappingCustomer = target.Value1;

            Mapper.Map(source).Over(target);

            target.Value1.ShouldBeSameAs(preMappingCustomer);
            target.Value1.Name.ShouldBe("Mystery :o");
            target.Value1.Address.ShouldNotBeNull();
            target.Value1.Address.Line1.ShouldBe("Over here");
            target.Value1.Address.Line2.ShouldBe("Over there");
            target.Value2.ShouldBe("Blimey!!");
        }
    }
}
