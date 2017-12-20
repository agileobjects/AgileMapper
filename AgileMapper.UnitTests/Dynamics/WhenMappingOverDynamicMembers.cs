namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
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

            ((ExpandoObject)result.Value).ShouldBeSameAs((ExpandoObject)targetDynamic);
            ((string)result.Value.Line1).ShouldBe("Here");
            ((string)result.Value.Line2).ShouldBe("There");
        }
    }
}
