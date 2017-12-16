namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Microsoft.CSharp.RuntimeBinder;
    using Shouldly;
    using TestClasses;
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

        [Fact]
        public void ShouldMapNestedMembersToAnExpandoObject()
        {
            var source = new Customer
            {
                Title = Title.Mrs,
                Name = "Captain Customer",
                Address = new Address { Line1 = "One!", Line2 = "Two!" }
            };
            dynamic result = Mapper.Map(source).ToANew<ExpandoObject>();

            ((object)result).ShouldNotBeNull();
            ((Title)result.Title).ShouldBe(Title.Mrs);
            ((string)result.Name).ShouldBe("Captain Customer");
            Should.Throw<RuntimeBinderException>(() => result.Address);
            ((string)result.Address_Line1).ShouldBe("One!");
            ((string)result.Address_Line2).ShouldBe("Two!");
        }
    }
}
