namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewComplexTypeMembers
    {
        [Fact]
        public void ShouldMapToNestedSimpleTypeMembers()
        {
            dynamic source = new ExpandoObject();
            source.Name = "Dynamic Customer!";
            source.Address = new Address
            {
                Line1 = "Dynamic Line 1!",
                Line2 = "Dynamic Line 2!",
            };

            var result = (Customer)Mapper.Map(source).ToANew<Customer>();

            result.Name = "Dynamic Customer!";
            result.Address.ShouldNotBeNull();
            result.Address.Line1.ShouldBe("Dynamic Line 1!");
            result.Address.Line2.ShouldBe("Dynamic Line 2!");
        }

        [Fact]
        public void ShouldMapFlattenedPropertiesToNestedSimpleTypeMembers()
        {
            dynamic source = new ExpandoObject();
            source.name = "Dynamic Person";
            source.addressLine1 = "Dynamic Line 1";
            source.addressLine2 = "Dynamic Line 2";

            var result = (Person)Mapper.Map(source).ToANew<Person>();

            result.Name = "Dynamic Person";
            result.Address.ShouldNotBeNull();
            result.Address.Line1.ShouldBe("Dynamic Line 1");
            result.Address.Line2.ShouldBe("Dynamic Line 2");
        }

        [Fact]
        public void ShouldMapANestedDynamicToANestedComplexTypeMember()
        {
            dynamic source = new ExpandoObject();

            source.Name = "Captain Dynamic";
            source.Address = new ExpandoObject();
            source.Address.Line1 = "Dynamic House";
            source.Address.Line2 = "Dynamic Street";

            var result = (Customer)Mapper.Map(source).ToANew<Customer>();

            result.Name.ShouldBe("Captain Dynamic");
            result.Address.ShouldNotBeNull();
            result.Address.Line1.ShouldBe("Dynamic House");
            result.Address.Line2.ShouldBe("Dynamic Street");
        }
    }
}