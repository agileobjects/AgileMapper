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
    }
}