namespace AgileObjects.AgileMapper.UnitTests.Dictionaries.Configuration
{
    using System;
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
                    .FromDictionaries
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
                    .FromDictionaries
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
                        .FromDictionaries
                        .To<Person>()
                        .Ignore(p => p.Id);

                    mapper.WhenMapping
                        .FromDictionaries
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
                        .FromDictionaries
                        .To<PublicField<string>>()
                        .Ignore(pf => pf.Value);

                    mapper.WhenMapping
                        .FromDictionaries
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
                        .FromDictionaries
                        .To<Person>()
                        .Map((d, p) => d.Count)
                        .To(p => p.Name);

                    mapper.WhenMapping
                        .FromDictionaries
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
                        .FromDictionaries
                        .To<Person>()
                        .Map((d, p) => d.Count)
                        .To(p => p.Name);

                    mapper.WhenMapping
                        .FromDictionaries
                        .To<Person>()
                        .MapMemberNameKey("PersonName")
                        .To(p => p.Name);
                }
            });

            configEx.Message.ShouldContain("has a configured data source");
        }

        [Fact]
        public void ShouldErrorIfAnInvalidSourceMemberIsSpecified()
        {
            var configEx = Should.Throw<NotSupportedException>(() =>
                Mapper.WhenMapping
                    .From<PublicField<string>>()
                    .ToDictionaries
                    .MapMember(pf => pf.Value + "!")
                    .ToFullKey("That won't work"));

            configEx.Message.ShouldContain("Unable to get member access");
        }

        [Fact]
        public void ShouldErrorIfAnUnreadableSourceMemberIsSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
                Mapper.WhenMapping
                    .From<PublicWriteOnlyProperty<string>>()
                    .ToDictionaries
                    .MapMember(pf => pf.Value)
                    .ToFullKey("That won't work"));

            configEx.Message.ShouldContain("is not readable");
        }

        [Fact]
        public void ShouldErrorIfRedundantSourceSeparatorIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .FromDictionaries
                        .UseMemberNameSeparator(".");
                }
            });

            configEx.Message.ShouldContain("already");
            configEx.Message.ShouldContain("global");
        }

        [Fact]
        public void ShouldErrorIfMemberNamesAreFlattenedAndSeparatedGlobally()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .FromDictionaries
                        .UseFlattenedTargetMemberNames()
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
                        .UseFlattenedTargetMemberNames();
                }
            });

            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("separated with '+'");
        }

        [Fact]
        public void ShouldErrorIfDifferentSeparatorsSpecifiedForASpecificTargetType()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .FromDictionaries
                        .To<PublicField<PublicProperty<string>>>()
                        .UseMemberNameSeparator("-")
                        .UseMemberNameSeparator("_");
                }
            });

            configEx.Message.ShouldContain("PublicField<PublicProperty<string>>");
            configEx.Message.ShouldContain("separated with '-'");
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
        public void ShouldErrorIfRedundantGlobalElementKeyPartIsConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .Dictionaries
                        .UseElementKeyPattern("[i]");
                }
            });

            configEx.Message.ShouldContain("already");
            configEx.Message.ShouldContain("global");
            configEx.Message.ShouldContain("[i]");
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

            configEx.Message.ShouldContain("must be constant string");
        }
    }
}
