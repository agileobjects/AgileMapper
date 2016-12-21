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
                    .FromDictionaries
                    .To<PublicField<string>>()
                    .MapKey(null)
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
                    .MapMemberName(null)
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
                        .MapMemberName("PersonId")
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
                        .MapMemberName("ValueValue")
                        .To(pf => pf.Value);
                }
            });

            configEx.Message.ShouldContain("has been ignored");
        }
    }
}
