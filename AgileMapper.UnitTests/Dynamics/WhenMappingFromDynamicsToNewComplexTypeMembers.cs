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
    }
}