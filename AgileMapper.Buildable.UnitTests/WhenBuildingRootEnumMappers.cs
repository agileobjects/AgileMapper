namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingRootEnumMappers
    {
        [Fact]
        public void ShouldBuildARootEnumMapper()
        {
            var enumIdSource = ((int)Title.Mrs).ToString();
            var enumIdResult = GeneratedMapper.Map(enumIdSource).ToANew<Title>();
            enumIdResult.ShouldBe(Title.Mrs);

            var enumLabelSource = Title.Master.ToString();
            var enumLabelResult = GeneratedMapper.Map(enumLabelSource).ToANew<Title>();
            enumLabelResult.ShouldBe(Title.Master);
        }

        #region Configuration

        public class RootEnumMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<string>().ToANew<Title>();
            }
        }

        #endregion
    }
}
