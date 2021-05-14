namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingComplexTypeMergeMappers
    {
        [Fact]
        public void ShouldBuildASingleSourceSingleTargetMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Address>().OnTo<Address>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Address { Line1 = "Line 1!" };
                var target = new Address { Line2 = "Line 2!" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                executor
                    .ShouldHaveAMergeMethod()
                    .ShouldExecuteAMergeMapping(target);

                target.Line1.ShouldBe("Line 1!");
                target.Line2.ShouldBe("Line 2!");
            }
        }
    }
}
