namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Api.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenApplyingMapperSetups
    {
        [Fact]
        public void ShouldApplyAMapperConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ApplyConfiguration.From<PfiToPfsMapperConfiguration>();

                var source = new PublicField<int> { Value = 123 };

                var result = mapper.Map(source).ToANew<PublicField<string>>();

                result.Value.ShouldBe(246);
            }
        }

        #region Helper Classes

        private class PfiToPfsMapperConfiguration : MapperConfiguration
        {
            protected override void Configure(MappingConfigStartingPoint whenMapping)
            {
                whenMapping
                    .From<PublicField<int>>()
                    .To<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(t => t.Value);
            }
        }

        #endregion
    }
}
