namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Common.TestClasses;
    using Xunit;

    public class WhenMappingOverDynamicMembers
    {
        [Fact]
        public void ShouldOverwriteASimpleTypeMember()
        {
            var source = new PublicField<Address>
            {
                Value = new Address { Line1 = "Here", Line2 = "There" }
            };

            dynamic targetDynamic = new ExpandoObject();
            targetDynamic.Line1 = "La la la";

            var target = new PublicProperty<dynamic> { Value = targetDynamic };

            var result = Mapper.Map(source).Over(target);

            Assert.Same(targetDynamic, result.Value);
            Assert.Equal("Here", result.Value.Line1);
            Assert.Equal("There", result.Value.Line2);
        }
    }
}
