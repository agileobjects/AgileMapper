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
    public class WhenConfiguringDataSourcesByFilterIncorrectly
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

            configEx.Message.ShouldContain("target member filter");
        }
    }
}
