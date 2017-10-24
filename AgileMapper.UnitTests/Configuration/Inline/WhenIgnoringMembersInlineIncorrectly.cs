namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersInlineIncorrectly
    {
        [Fact]
        public void ShouldErrorIfConfiguredDataSourceMemberIsIgnoredInline()
        {
            var inlineConfigEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .Map(new PublicField<string> { Value = "Hello" })
                        .ToANew<PublicField<string>>(cfg => cfg
                            .Map(ctx => ctx.Source.Value + "?!")
                            .To(pf => pf.Value)
                            .And
                            .Ignore(pf => pf.Value));
                }
            });

            inlineConfigEx.Message.ShouldContain("Ignored member Target.Value has a configured data source");
        }

        [Fact]
        public void ShouldErrorIfFilteredMemberIsIgnoredInline()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(member => member.IsField);

                    mapper
                        .Map(new PublicField<long> { Value = 123 })
                        .ToANew<PublicField<int>>(cfg => cfg
                            .Ignore(pf => pf.Value));
                }
            });

            ignoreEx.Message.ShouldContain("Already ignored by ignore pattern");
        }

        [Fact]
        public void ShouldErrorIfDuplicateFilterIsConfiguredInline()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreTargetMembersWhere(m => m.IsPropertyMatching(pi => pi.CanWrite));

                    mapper
                        .Map(new PublicField<long> { Value = 123 })
                        .ToANew<PublicField<int>>(cfg => cfg
                            .IgnoreTargetMembersWhere(m => m.IsPropertyMatching(pi => pi.CanWrite)));
                }
            });

            ignoreEx.Message.ShouldContain("has already been configured");
        }
    }
}