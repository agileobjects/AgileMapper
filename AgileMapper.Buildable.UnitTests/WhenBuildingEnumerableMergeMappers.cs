namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System.Collections.Generic;
    using AgileMapper.UnitTests.Common;
    using Xunit;

    public class WhenBuildingEnumerableMergeMappers
    {
        [Fact]
        public void ShouldBuildASimpleTypeIEnumerableToICollectionMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<IEnumerable<int>>().OnTo<ICollection<int>>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                IEnumerable<int> source = new[] { 4, 5, 6 };
                ICollection<int> target = new[] { 1, 2, 3 };

                var result = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass()
                    .GetMapMethods()
                    .ShouldHaveSingleItem()
                    .ShouldCreateMappingExecutor(source)
                    .ShouldHaveAMergeMethod()
                    .ShouldExecuteAMergeMapping(target);

                result.ShouldNotBeNull().ShouldNotBeSameAs(target);
                result.ShouldBe(1, 2, 3, 4, 5, 6);
            }
        }

        [Fact]
        public void ShouldBuildASimpleTypeArrayToHashSetMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<decimal[]>().OnTo<HashSet<double>>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var source = new[] { 1.0m, 2.0m, 3.0m };
                var target = new HashSet<double> { 2.0, 3.0, 4.0 };

                var result = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass()
                    .GetMapMethods()
                    .ShouldHaveSingleItem()
                    .ShouldCreateMappingExecutor(source)
                    .ShouldHaveAMergeMethod()
                    .ShouldExecuteAMergeMapping(target);

                result.ShouldNotBeNull().ShouldBe(2.0, 3.0, 4.0, 1.0);
            }
        }
    }
}
