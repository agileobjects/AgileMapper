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
            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "There" };

            var result = Mapper.Map(source).ToNew<Customer>();

            result.Address.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.AddressLine1)
                    .To(x => x.Address.Line1);

                var source = new PersonViewModel { Name = "Fred", AddressLine1 = "Lala Land" };
                var result = mapper.Map(source).ToNew<Person>();

                result.Address.Line1.ShouldBe("Fred, Lala Land");
            }
        }
    }
}
