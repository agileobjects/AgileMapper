namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingRootEnumMappers
    {
        [Fact]
        public void ShouldBuildARootEnumMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<string>().ToANew<Title>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var enumIdSource = ((int)Title.Mrs).ToString();

                var enumIdExecutor = staticMapMethod
                    .ShouldCreateMappingExecutor(enumIdSource);

                var enumIdResult = enumIdExecutor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Title>();

                enumIdResult.ShouldBe(Title.Mrs);

                var enumLabelSource = Title.Master.ToString();

                var enumLabelIdExecutor = staticMapMethod
                    .ShouldCreateMappingExecutor(enumLabelSource);

                var enumLabelResult = enumLabelIdExecutor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Title>();

                enumLabelResult.ShouldBe(Title.Master);
            }
        }
    }
}
