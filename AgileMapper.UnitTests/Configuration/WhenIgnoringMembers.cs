namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Configuration;
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
                mapper.WhenMapping
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
                mapper.WhenMapping
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
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .If(ctx => ctx.Source.Name == "Bilbo")
                    .Ignore(x => x.Name);

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
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .If(ctx => ctx.Source.Name.StartsWith("F"))
                    .Ignore(p => p.Name);

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
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<string>>()
                    .If(ctx => ctx.EnumerableIndex > 0)
                    .Ignore(p => p.Value);

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

        [Fact]
        public void ShouldErrorIfRedundantIgnoreIsSpecified()
        {
            Assert.Throws<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.Create())
                {
                    mapper
                        .WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Ignore(pvm => pvm.Name);

                    mapper
                        .WhenMapping
                        .From<Customer>()
                        .To<CustomerViewModel>()
                        .Ignore(cvm => cvm.Name);
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfRedundantConditionalIgnoreConflictsWithIgnore()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(pvm => pvm.Name);

                mapper
                    .WhenMapping
                    .From<Customer>()
                    .To<CustomerViewModel>()
                    .If((c, cvm) => c.Name == "Frank")
                    .Ignore(cvm => cvm.Name);
            }
        }

        [Fact]
        public void ShouldNotErrorIfRedundantIgnoreConflictsWithConditionalIgnore()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .If((p, pvm) => p.Name == "Frank")
                    .Ignore(pvm => pvm.Name);

                mapper
                    .WhenMapping
                    .From<Customer>()
                    .To<CustomerViewModel>()
                    .Ignore(cvm => cvm.Name);
            }
        }

        [Fact]
        public void ShouldErrorIfConfiguredDataSourceMemberIsIgnored()
        {
            Assert.Throws<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.Create())
                {
                    mapper
                        .WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Map((p, pvm) => p.Title + " " + p.Name)
                        .To(pvm => pvm.Name);

                    mapper
                        .WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Ignore(cvm => cvm.Name);
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfSamePathIgnoredMembersHaveDifferentSourceTypes()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .Ignore(x => x.Value);

                mapper
                    .WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicProperty<int>>()
                    .Ignore(x => x.Value);
            }
        }

        [Fact]
        public void ShouldNotErrorIfSamePathIgnoredMembersHaveDifferentTargetTypes()
        {
            using (var mapper = Mapper.Create())
            {
                mapper
                    .WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(x => x.Name);

                mapper
                    .WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Ignore(x => x.Name);
            }
        }
    }
}