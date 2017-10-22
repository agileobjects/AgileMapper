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

                var personResult = mapper.Map(new Person { Name = "Dennis" }).ToANew<PersonViewModel>();
                var matchingCustomerResult = mapper.Map(new Customer { Name = "Mac" }).ToANew<CustomerViewModel>();
                var nonMatchingCustomerResult = mapper.Map(new Customer { Name = "Frank" }).ToANew<CustomerViewModel>();

                personResult.Name.ShouldBeNull();
                matchingCustomerResult.Name.ShouldBe("Mac");
                nonMatchingCustomerResult.Name.ShouldBeNull();
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

            configurationEx.Message.ShouldContain("not mappable");
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
        public void ShouldErrorIfDuplicateFilterIsConfigured()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(member => member.IsField);

                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(member => member.IsField);
                }
            });

            ignoreEx.Message.ShouldContain("has already been configured");
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