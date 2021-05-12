namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.UnitTests.Common;
    using Plans;
    using Xunit;

    public class WhenBuildingEnumerableCreateNewMappers
    {
        [Fact]
        public void ShouldBuildSimpleTypeListToCollectionMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<List<string>>().ToANew<Collection<byte?>>(new MappingPlanSettings
                {
                    LazyCompile = true
                });

                var sourceCodeExpressions = mapper.BuildSourceCode();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new List<string> { "3", "2", "1", "12345" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Collection<byte?>>(executor);

                result.ShouldNotBeNull();
                result.ShouldBe<byte?>(3, 2, 1, null);
            }
        }
    }
}
