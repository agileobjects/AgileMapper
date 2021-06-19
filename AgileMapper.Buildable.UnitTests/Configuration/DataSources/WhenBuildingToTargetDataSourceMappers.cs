namespace AgileObjects.AgileMapper.Buildable.UnitTests.Configuration.DataSources
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingToTargetDataSourceMappers
    {
        [Fact]
        public void ShouldApplyAToTargetDataSource()
        {
            var source = new ToTargetValueSource<int, string, int>
            {
                Value1 = 123,
                Value = new PublicTwoFields<string, int>
                {
                    Value1 = "456",
                    Value2 = 789
                }
            };

            var result = GeneratedMapper.Map(source).ToANew<PublicTwoFields<int, int>>();

            result.Value1.ShouldBe(456);
            result.Value2.ShouldBe(789);
        }

        #region Configuration

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

        #endregion
    }

    public class ToTargetValueSource<T1, T2, T3>
    {
        public T1 Value1 { get; set; }

        public PublicTwoFields<T2, T3> Value { get; set; }
    }
}
