namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDictionaryMappingIncorrectly
    {
        [Fact]
        public void ShouldErrorIfCustomMemberKeyIsNull()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                Mapper.WhenMapping
                    .Dictionaries
                    .To<PublicField<string>>()
                    .MapFullKey(null)
                    .To(pf => pf.Value);
            });

            configEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfCustomMemberNameIsNull()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                Mapper.WhenMapping
                    .Dictionaries
                    .To<PublicField<string>>()
                    .MapMemberNameKey(null)
                    .To(pf => pf.Value);
            });

            configEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfIgnoredMemberIsGivenCustomMemberKey()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .Ignore(p => p.Id);

                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .MapFullKey("PersonId")
                        .To(p => p.Id);
                }
            });

            configEx.Message.ShouldContain("has been ignored");
        }

        [Fact]
        public void ShouldErrorIfIgnoredMemberIsGivenCustomMemberName()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<PublicField<string>>()
                        .Ignore(pf => pf.Value);

                    mapper.WhenMapping
                        .Dictionaries
                        .To<PublicField<string>>()
                        .MapMemberNameKey("ValueValue")
                        .To(pf => pf.Value);
                }
            });

            configEx.Message.ShouldContain("has been ignored");
        }

        [Fact]
        public void ShouldErrorIfCustomDataSourceMemberIsGivenCustomMemberKey()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .Map((d, p) => d.Count)
                        .To(p => p.Name);

                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .MapFullKey("PersonName")
                        .To(p => p.Name);
                }
            });

            configEx.Message.ShouldContain("has a configured data source");
        }

        [Fact]
        public void ShouldErrorIfCustomDataSourceMemberIsGivenCustomMemberName()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .Map((d, p) => d.Count)
                        .To(p => p.Name);

                    mapper.WhenMapping
                        .Dictionaries
                        .To<Person>()
                        .MapMemberNameKey("PersonName")
                        .To(p => p.Name);
                }
            });

            configEx.Message.ShouldContain("has a configured data source");
        }

        [Fact]
        public void ShouldErrorIfMemberNamesAreFlattenedAndSeparatedGlobally()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .UseFlattenedMemberNames()
                        .UseMemberNameSeparator("+");
                }
            });

            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("flattened");
        }

        [Fact]
        public void ShouldErrorIfMemberNamesAreSeparatedAndFlattenedGlobally()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .UseMemberNameSeparator("+")
                        .UseFlattenedMemberNames();
                }
            });

            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("separated with '+'");
        }

        [Fact]
        public void ShouldErrorIfMemberNamesAreFlattenedAndSeparatedForASpecificTargetType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<PublicField<PublicProperty<string>>>()
                        .UseFlattenedMemberNames()
                        .UseMemberNameSeparator("_");
                }
            });

            configEx.Message.ShouldContain("PublicField<PublicProperty<string>>");
            configEx.Message.ShouldContain("flattened");
        }

        [Fact]
        public void ShouldErrorIfMemberNamesAreSeparatedAndFlattenedForASpecificTargetType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .To<PublicProperty<PublicField<int>>>()
                        .UseMemberNameSeparator("+")
                        .UseFlattenedMemberNames();
                }
            });

            configEx.Message.ShouldContain("PublicProperty<PublicField<int>>");
            configEx.Message.ShouldContain("separated with '+'");
        }

        [Fact]
        public void ShouldErrorIfANullElementKeyPartIsSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .Dictionaries
                    .UseElementKeyPattern(null));

            configEx.Message.ShouldContain(
                "pattern must contain a single 'i' character as a placeholder for the enumerable index");
        }

        [Fact]
        public void ShouldErrorIfABlankElementKeyPartIsSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .Dictionaries
                    .UseElementKeyPattern(string.Empty));

            configEx.Message.ShouldContain(
                "pattern must contain a single 'i' character as a placeholder for the enumerable index");
        }

        [Fact]
        public void ShouldErrorIfAnElementKeyPartHasNoIndexPlaceholder()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .Dictionaries
                    .UseElementKeyPattern("_x_"));

            configEx.Message.ShouldContain(
                "pattern must contain a single 'i' character as a placeholder for the enumerable index");
        }

        [Fact]
        public void ShouldErrorIfAnElementKeyPartHasMultipleIndexPlaceholders()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .Dictionaries
                    .UseElementKeyPattern("ii"));

            configEx.Message.ShouldContain(
                "pattern must contain a single 'i' character as a placeholder for the enumerable index");
        }

        [Fact]
        public void ShouldErrorIfCustomTargetMemberKeyIsNotAConstant()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .ToDictionaries
                    .Map(ctx => ctx.EnumerableIndex)
                    .To(d => d[d.Count.ToString()]));

            configEx.Message.ShouldContain("must be constant");
        }
    }
}
