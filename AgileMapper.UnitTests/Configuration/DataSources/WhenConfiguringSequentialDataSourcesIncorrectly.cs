namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using System;
    using AgileMapper.Configuration;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
    using static WhenConfiguringSequentialDataSources;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static WhenConfiguringSequentialDataSources;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringSequentialDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfDuplicateSequentialDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Issue184.SourcePets>()
                        .ToANew<Issue184.TargetPets>()
                        .Map((src, _) => src.TheCat)
                        .Then.Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames);
                }
            });

            configEx.Message.ShouldContain("already has configured data source");
            configEx.Message.ShouldContain("TheCat");
        }

        [Fact]
        public void ShouldErrorIfSequentialDataSourceMemberDuplicated()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Issue184.SourcePets>()
                        .ToANew<Issue184.TargetPets>()
                        .Map((src, _) => src.TheCat)
                        .Then.Map((src, _) => src.TheDog)
                        .To(tp => tp.PetNames)
                        .And
                        .Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames);
                }
            });

            configEx.Message.ShouldContain("already has configured data source");
            configEx.Message.ShouldContain("TheCat");
        }

        [Fact]
        public void ShouldErrorIfSimpleTypeMemberSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<string, string>>()
                        .To<PublicProperty<DateTime>>()
                        .Map(ctx => ctx.Source.Value1)
                        .Then.Map(ctx => ctx.Source.Value2)
                        .To(pp => pp.Value);
                }
            });

            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value2");
            configEx.Message.ShouldContain("cannot be sequentially applied");
            configEx.Message.ShouldContain("PublicProperty<DateTime>.Value");
            configEx.Message.ShouldContain("cannot have sequential data sources");
        }

        [Fact]
        public void ShouldErrorIfIgnoredSourceMemberSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<Address, Address>>()
                        .ToANew<PublicProperty<Address>>()
                        .IgnoreSource(ptf => ptf.Value2)
                        .And
                        .Map((ptf, _) => ptf.Value1)
                        .Then.Map((ptf, _) => ptf.Value2)
                        .To(pp => pp.Value);
                }
            });

            configEx.Message.ShouldContain("PublicTwoFields<Address, Address>.Value2");
            configEx.Message.ShouldContain("PublicProperty<Address>.Value");
            configEx.Message.ShouldContain("conflicts with an ignored source member");
        }
    }
}