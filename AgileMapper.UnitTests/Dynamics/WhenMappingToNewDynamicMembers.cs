namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
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

            Assert.NotNull(result.Value);

            dynamic resultValue = result.Value;
            Assert.Equal("Over here!", resultValue.Line1);
            Assert.Equal("Over there!", resultValue.Line2);
            Assert.Equal("Over where?!", resultValue.Line3);
        }

        [Fact]
        public void ShouldMapFromNestedMembers()
        {
            var source = new PublicField<Address>
            {
                Value = new Address
                {
                    Line1 = "One One One",
                    Line2 = "Two Two Two"
                }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<ExpandoObject>>();

            Assert.NotNull(result.Value);

            dynamic resultValue = result.Value;
            Assert.Equal("One One One", resultValue.Line1);
            Assert.Equal("Two Two Two", resultValue.Line2);
        }
    }
}
