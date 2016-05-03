namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembers
    {
        [Fact]
        public void ShouldIgnoreAConfiguredMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(x => x.Name);

                var source = new PersonViewModel { Name = "Jon" };
                var result = mapper.Map(source).ToNew<Person>();

                result.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberInARootCollection()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<Person>()
                    .Over<Person>()
                    .Ignore(x => x.Address);

                var source = new[] { new Person { Name = "Jon", Address = new Address { Line1 = "Blah" } } };
                var target = new[] { new Person() };
                var result = mapper.Map(source).Over(target);

                result.Length.ShouldBe(1);
                result.First().Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(x => x.Name)
                    .If((s, t) => s.Name == "Bilbo");

                var matchingSource = new PersonViewModel { Name = "Bilbo" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                matchingResult.Name.ShouldBeNull();

                var nonMatchingSource = new PersonViewModel { Name = "Frodo" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<Person>();

                nonMatchingResult.Name.ShouldBe("Frodo");
            }
        }
    }
}