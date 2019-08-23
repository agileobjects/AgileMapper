namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
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
    public class WhenIgnoringSourceMemberInlineIncorrectly
    {
        [Fact]
        public void ShouldErrorIfDuplicateSourceIgnoreIsConfiguredInline()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicField<int>>()
                        .IgnoreSource(pf => pf.Value);

                    mapper
                        .Map(new PublicField<int> { Value = 123 })
                        .ToANew<PublicField<long>>(cfg => cfg
                            .IgnoreSource(pf => pf.Value));
                }
            });

            ignoreEx.Message.ShouldContain("has already been ignored");
        }
    }
}
