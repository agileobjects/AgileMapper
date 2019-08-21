namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using Common;
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
