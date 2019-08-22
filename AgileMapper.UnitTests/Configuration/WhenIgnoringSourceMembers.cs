namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembers
    {
        [Fact]
        public void ShouldIgnoreAConfiguredSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<IdTesterSource>()
                    .ToANew<IdTesterTarget>()
                    .IgnoreSource(id => id.Id);

                var source = new IdTesterSource { Id = "Id!", Identifier = "Identifier!" };
                var result = mapper.Map(source).ToANew<IdTesterTarget>();

                result.Id.ShouldBe("Identifier!");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredSourceMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicField<string>>()
                    .If(ctx => ctx.Source.Value < 5)
                    .IgnoreSource(pf => pf.Value);

                var matchingSource = new PublicField<int> { Value = 3 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<string>>();

                matchingResult.Value.ShouldBeNull();

                var nonMatchingSource = new PublicField<int> { Value = 7 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<string>>();

                nonMatchingResult.Value.ShouldBe("7");
            }
        }

        #region Helper Classes

        private class IdTesterSource
        {
            public string Id { get; set; }

            public string Identifier { get; set; }
        }

        private class IdTesterTarget
        {
            public string Id { get; set; }
        }

        #endregion
    }
}
