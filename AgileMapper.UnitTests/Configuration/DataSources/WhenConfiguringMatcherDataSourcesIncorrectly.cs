namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
{
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    // See https://github.com/agileobjects/AgileMapper/issues/208
    public class WhenConfiguringMatcherDataSourcesIncorrectly
    {
        [Fact]
        public void ShouldErrorIfMemberIgnoreSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ToANew<PublicTwoFields<string, string>>()
                        .IfTargetMembersMatch(member => member.IsField)
                        .Ignore(ptf => ptf.Value1, ptf => ptf.Value2);
                }
            });

            configEx.Message.ShouldContain("target member filter");
            configEx.Message.ShouldContain("member.IsField");
            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value1,");
            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value2");
        }

        [Fact]
        public void ShouldErrorIfDataSourceTargetMemberSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ToANew<PublicTwoFields<string, string>>()
                        .IfTargetMembersMatch(member => member.IsProperty)
                        .Map("Yippee!")
                        .To(ptf => ptf.Value1);
                }
            });

            configEx.Message.ShouldContain("target member filter");
            configEx.Message.ShouldContain("member.IsProperty");
            configEx.Message.ShouldContain("data source mapping '\"Yippee!\"' -> ");
            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value1");
        }

        [Fact]
        public void ShouldErrorIfConflictingMatcherDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<string>().To<bool>()
                        .IfTargetMembersMatch(member => member.Name == "AlwaysTrue")
                        .Map(true).ToTarget();

                    mapper.WhenMapping
                        .From<string>().To<bool>()
                        .IfTargetMembersMatch(member => member.Name == "AlwaysTrue")
                        .Map(false).ToTarget();
                }
            });

            configEx.Message.ShouldContain("already has");
            configEx.Message.ShouldContain("'If mapping string -> bool and member.Name == \"AlwaysTrue\",");
            configEx.Message.ShouldContain("map 'true' to target'");
        }

        [Fact]
        public void ShouldErrorIfConflictingMemberFilterConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ToANew<PublicTwoFields<string, string>>()
                        .IfTargetMembersMatch(member => member.HasType<string>())
                        .Map("Yippee!")
                        .ToTarget();

                    mapper.WhenMapping
                        .ToANew<PublicTwoFields<string, string>>()
                        .IgnoreTargetMembersOfType<string>();
                }
            });

            configEx.Message.ShouldContain("'If mapping -> PublicTwoFields<string, string>");
            configEx.Message.ShouldContain("and member.HasType<string>(), map '\"Yippee!\"' to target member'");
            configEx.Message.ShouldContain("member ignore pattern 'member.HasType<string>()'");
        }

        [Fact]
        public void ShouldErrorIfRedundantDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>().To<PublicField<string>>()
                        .IfTargetMembersMatch(member => member.HasType<string>())
                        .Map((s, t) => s.Value * 3).ToTarget();

                    mapper.WhenMapping
                        .From<PublicField<int>>().To<PublicField<string>>()
                        .Map((s, t) => s.Value * 3)
                        .To(t => t.Value);
                }
            });

            configEx.Message.ShouldContain("PublicField<string>.Value already has");
            configEx.Message.ShouldContain("'If mapping PublicField<int> -> PublicField<string> ");
            configEx.Message.ShouldContain("and member.HasType<string>(),");
            configEx.Message.ShouldContain("map 's.Value * 3' to target member'");
        }
    }
}
