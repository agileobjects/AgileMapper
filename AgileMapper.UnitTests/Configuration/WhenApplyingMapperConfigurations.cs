namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenApplyingMapperConfigurations
    {
        [Fact]
        public void ShouldApplyAMapperConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseConfiguration.From<PfiToPfsMapperConfiguration>();

                var source = new PublicField<int> { Value = 123 };

                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(246);
            }
        }

        #region Helper Classes

        private class PfiToPfsMapperConfiguration : MapperConfiguration
        {
            protected override void Configure()
            {
                WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(t => t.Value);

                GetPlanFor<PublicField<int>>().ToANew<PublicField<string>>();
            }
        }

        #endregion
    }
}
