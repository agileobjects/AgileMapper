namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewComplexTypeMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new Person
            {
                Address = new Address
                {
                    Line1 = "Over here!"
                }
            };

            var result = Mapper.Map(source).ToNew<Person>();

            result.Address.ShouldNotBeNull();
            result.Address.ShouldNotBe(source.Address);
            result.Address.Line1.ShouldBe(source.Address.Line1);
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new Person { Name = "Freddie" };

            var result = Mapper.Map(source).ToNew<Person>();

            result.Name.ShouldBe(source.Name);
            result.Address.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "There" };

            var result = Mapper.Map(source).ToNew<Customer>();

            result.Address.ShouldNotBeNull();
        }
    }
}
