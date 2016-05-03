namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
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
        public void ShouldConditionallyIgnoreAConfiguredMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(x => x.Name)
                    .If((pvm, p) => pvm.Name == "Bilbo");

                var matchingSource = new PersonViewModel { Name = "Bilbo" };
                var matchingResult = mapper.Map(matchingSource).ToNew<Person>();

                matchingResult.Name.ShouldBeNull();

                var nonMatchingSource = new PersonViewModel { Name = "Frodo" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<Person>();

                nonMatchingResult.Name.ShouldBe("Frodo");
            }
        }

        [Fact]
        public void ShouldConditionallyIgnoreAConfiguredMemberInACollection()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(p => p.Name)
                    .If(pvm => pvm.Name.StartsWith("F"));

                var source = new[]
                {
                    new PersonViewModel { Name = "Bilbo" },
                    new PersonViewModel { Name = "Frodo" }
                };

                var result = mapper.Map(source).ToNew<IEnumerable<Person>>();

                result.Count().ShouldBe(2);

                result.First().Name.ShouldBe("Bilbo");
                result.Second().Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldConditionallyIgnoreAConfiguredMemberByEnumerableElement()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<string>>()
                    .Ignore(p => p.Value)
                    .If((s, t, i) => i > 0);

                var source = new[]
                {
                    new PublicProperty<int> { Value = 123 },
                    new PublicProperty<int> { Value = 456 }
                };

                var result = mapper.Map(source).ToNew<IEnumerable<PublicProperty<string>>>();

                result.Count().ShouldBe(2);

                result.First().Value.ShouldBe("123");
                result.Second().Value.ShouldBeNull();
            }
        }
    }
}