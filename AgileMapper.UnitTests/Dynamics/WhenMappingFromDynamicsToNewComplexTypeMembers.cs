#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Common.TestClasses;
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

            var result = Mapper.Map(source).ToANew<Customer>();

            Assert.Equal("Dynamic Customer!", result.Name);
            Assert.NotNull(result.Address);
            Assert.Equal("Dynamic Line 1!", result.Address.Line1);
            Assert.Equal("Dynamic Line 2!", result.Address.Line2);
        }

        [Fact]
        public void ShouldMapFlattenedPropertiesToNestedSimpleTypeMembers()
        {
            dynamic source = new ExpandoObject();
            source.name = "Dynamic Person";
            source.addressLine1 = "Dynamic Line 1";
            source.addressLine2 = "Dynamic Line 2";

            var result = Mapper.Map(source).ToANew<Person>();

            Assert.Equal("Dynamic Person", result.Name);
            Assert.NotNull(result.Address);
            Assert.Equal("Dynamic Line 1", result.Address.Line1);
            Assert.Equal("Dynamic Line 2", result.Address.Line2);
        }

        [Fact]
        public void ShouldMapANestedDynamicToANestedComplexTypeMember()
        {
            dynamic source = new ExpandoObject();

            source.Name = "Captain Dynamic";
            source.Address = new ExpandoObject();
            source.Address.Line1 = "Dynamic House";
            source.Address.Line2 = "Dynamic Street";

            var result = Mapper.Map(source).ToANew<Customer>();

            Assert.Equal("Captain Dynamic", result.Name);
            Assert.NotNull(result.Address);
            Assert.Equal("Dynamic House", result.Address.Line1);
            Assert.Equal("Dynamic Street", result.Address.Line2);
        }
    }
}
#endif