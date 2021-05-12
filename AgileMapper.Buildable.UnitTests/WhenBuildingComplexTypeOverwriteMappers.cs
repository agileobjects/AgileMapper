namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingComplexTypeOverwriteMappers
    {
        [Fact]
        public void ShouldBuildASingleSourceSingleTargetMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Address>().Over<Address>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Address { Line1 = "1.1", Line2 = "1.2" };
                var target = new Address { Line1 = "2.1", Line2 = "2.2" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                executor
                    .ShouldHaveAnOverwriteMethod()
                    .ShouldExecuteAnOverwriteMapping(executor, target);

                target.Line1.ShouldBe("1.1");
                target.Line2.ShouldBe("1.2");
            }
        }
    }
}