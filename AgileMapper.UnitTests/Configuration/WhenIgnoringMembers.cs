namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembers
    {
        [Fact]
        public void ShouldIgnoreAConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(x => x.Name);

                var source = new PersonViewModel { Name = "Jon" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberInARootCollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<Person>()
                    .Ignore(p => p.Address);

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
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .If(ctx => ctx.Source.Name == "Bilbo")
                    .Ignore(x => x.Name);

                var matchingSource = new PersonViewModel { Name = "Bilbo" };
                var matchingResult = mapper.Map(matchingSource).ToANew<Person>();

                matchingResult.Name.ShouldBeNull();

                var nonMatchingSource = new PersonViewModel { Name = "Frodo" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Person>();

                nonMatchingResult.Name.ShouldBe("Frodo");
            }
        }

        [Fact]
        public void ShouldConditionallyIgnoreAConfiguredMemberForASpecifiedRuleSet()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .OnTo<Address>()
                    .If(ctx => ctx.Source.Name == "Gandalf")
                    .Ignore(a => a.Line1);

                var source = new PersonViewModel { Name = "Gandalf", AddressLine1 = "??" };
                var onToResult = mapper.Map(source).OnTo(new Person { Address = new Address() });

                onToResult.Name.ShouldBe("Gandalf");
                onToResult.Address.Line1.ShouldBeNull();

                var createNewResult = mapper.Map(source).ToANew<Person>();

                createNewResult.Name.ShouldBe("Gandalf");
                createNewResult.Address.Line1.ShouldBe("??");
            }
        }

        [Fact]
        public void ShouldIgnoreMultipleConfiguredMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(p => p.Name, p => p.Address.Line1);

                var source = new PersonViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Bilbo",
                    AddressLine1 = "House Street"
                };
                var matchingResult = mapper.Map(source).ToANew<Person>();

                matchingResult.Id.ShouldBe(source.Id);
                matchingResult.Name.ShouldBeNull();
                matchingResult.Address.ShouldNotBeNull();
                matchingResult.Address.Line1.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldConditionallyIgnoreAConfiguredMemberInACollection()
        {
            using (var mapper = Mapper.CreateNew())
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

                var result = mapper.Map(source).ToANew<IEnumerable<Person>>();

                result.Count().ShouldBe(2);

                result.First().Name.ShouldBe("Bilbo");
                result.Second().Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldConditionallyIgnoreAConfiguredMemberByEnumerableElement()
        {
            using (var mapper = Mapper.CreateNew())
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

                var result = mapper.Map(source).ToANew<IEnumerable<PublicProperty<string>>>();

                result.Count().ShouldBe(2);

                result.First().Value.ShouldBe("123");
                result.Second().Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldErrorIfRedundantIgnoreIsSpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
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
            using (var mapper = Mapper.CreateNew())
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
            using (var mapper = Mapper.CreateNew())
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
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
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
            using (var mapper = Mapper.CreateNew())
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
            using (var mapper = Mapper.CreateNew())
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

        [Fact]
        public void ShouldErrorIfNonPublicReadOnlySimpleTypeMemberSpecified()
        {
            var configurationEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<PublicSetMethod<string>>()
                        .Ignore(psm => psm.Value);
                }
            });

            configurationEx.Message.ShouldContain("not writeable");
        }

        [Fact]
        public void ShouldErrorIfReadOnlySimpleTypeMemberSpecified()
        {
            var configurationEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<PublicReadOnlyField<int>>()
                        .Ignore(psm => psm.Value);
                }
            });

            configurationEx.Message.ShouldContain("not writeable");
        }

        [Fact]
        public void ShouldNotErrorIfReadOnlyComplexTypeMemberSpecified()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicReadOnlyProperty<Address>>()
                    .Ignore(psm => psm.Value);
            }
        }
    }
}