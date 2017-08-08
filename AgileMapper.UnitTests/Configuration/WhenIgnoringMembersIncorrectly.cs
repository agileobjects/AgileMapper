namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersIncorrectly
    {
        [Fact]
        public void ShouldErrorIfRedundantIgnoreIsSpecified()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Ignore(pvm => pvm.Name);

                    mapper.WhenMapping
                        .From<Customer>()
                        .To<CustomerViewModel>()
                        .Ignore(cvm => cvm.Name);
                }
            });

            ignoreEx.Message.ShouldContain("has already been ignored");
        }

        [Fact]
        public void ShouldNotErrorIfRedundantConditionalIgnoreConflictsWithIgnore()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(pvm => pvm.Name);

                mapper.WhenMapping
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
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .If((p, pvm) => p.Name == "Frank")
                    .Ignore(pvm => pvm.Name);

                mapper.WhenMapping
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
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Map((p, pvm) => p.Title + " " + p.Name)
                        .To(pvm => pvm.Name);

                    mapper.WhenMapping
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
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .Ignore(x => x.Value);

                mapper.WhenMapping
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
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(x => x.Name);

                mapper.WhenMapping
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

        [Fact]
        public void ShouldErrorIfFilteredMemberIsIgnored()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(member => member.IsField);

                    mapper.WhenMapping
                        .To<PublicField<int>>()
                        .Ignore(pf => pf.Value);
                }
            });

            ignoreEx.Message.ShouldContain("Already ignored by ignore pattern");
        }

        [Fact]
        public void ShouldErrorIfConfiguredDataSourceMemberIsFiltered()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PersonViewModel>()
                        .Map((p, pvm) => p.Title + ". " + p.Name)
                        .To(pvm => pvm.Name);

                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(member => member.Name == "Name");
                }
            });

            configEx.Message.ShouldContain("member.Name == \"Name\"");
        }
    }
}