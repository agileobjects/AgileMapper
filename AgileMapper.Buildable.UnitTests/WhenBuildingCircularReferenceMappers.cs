namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingCircularReferenceMappers
    {
        [Fact]
        public void ShouldBuildACircularReferenceMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Child>().ToANew<Child>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Child
                {
                    Name = "Fred",
                    EldestParent = new Parent
                    {
                        Name = "Bonnie",
                        EldestChild = new Child
                        {
                            Name = "Samson",
                            EldestParent = new Parent
                            {
                                Name = "Franklin"
                            }
                        }
                    }
                };

                source.EldestParent.EldestChild.EldestParent.EldestChild = source;

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Child>();

                result.ShouldNotBeNull().ShouldNotBeSameAs(source);

                result.Name.ShouldBe("Fred");
                result.EldestParent.ShouldNotBeNull();

                result.EldestParent.Name.ShouldBe("Bonnie");
                result.EldestParent.EldestChild.ShouldNotBeNull();

                result.EldestParent.EldestChild.Name.ShouldBe("Samson");
                result.EldestParent.EldestChild.EldestParent.ShouldNotBeNull();

                result.EldestParent.EldestChild.EldestParent.Name.ShouldBe("Franklin");
                result.EldestParent.EldestChild.EldestParent.EldestChild.ShouldBeSameAs(result);
            }
        }
    }
}