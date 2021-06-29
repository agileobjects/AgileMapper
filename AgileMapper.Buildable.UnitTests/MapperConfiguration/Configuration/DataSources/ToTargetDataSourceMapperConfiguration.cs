namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration.Configuration.DataSources
{
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class ToTargetDataSourceMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlansFor<ToTargetValueSource<int, string, int>>()
                .To<PublicTwoFields<int, int>>(cfg => cfg
                    .Map(ctx => ctx.Source.Value)
                    .ToTarget());
        }
    }
}