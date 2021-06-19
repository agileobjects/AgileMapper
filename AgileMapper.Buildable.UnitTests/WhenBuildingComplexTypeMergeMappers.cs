namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingComplexTypeMergeMappers
    {
        [Fact]
        public void ShouldBuildASingleSourceSingleTargetMapper()
        {
            var source = new Address { Line1 = "Line 1!" };
            var target = new Address { Line2 = "Line 2!" };

            GeneratedMapper.Map(source).OnTo(target);

            target.Line1.ShouldBe("Line 1!");
            target.Line2.ShouldBe("Line 2!");
        }
    }
}
