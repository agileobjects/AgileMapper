namespace AgileObjects.AgileMapper.Buildable.UnitTests.Configuration.DataSources
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
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
    }
}
