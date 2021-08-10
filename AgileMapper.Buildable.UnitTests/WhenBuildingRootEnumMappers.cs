namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Mappers.Extensions;
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

            const string ENUM_LABEL_SOURCE = nameof(Title.Master);
            var enumLabelResult = GeneratedMapper.Map(ENUM_LABEL_SOURCE).ToANew<Title>();
            enumLabelResult.ShouldBe(Title.Master);
        }

        [Fact]
        public void ShouldBuildARootEnumMappingExtensionMethod()
        {
            const string ENUM_LABEL_SOURCE = nameof(Title.Count);
            var enumLabelResult = ENUM_LABEL_SOURCE.Map().ToANew<Title>();
            enumLabelResult.ShouldBe(Title.Count);
        }
    }
}
