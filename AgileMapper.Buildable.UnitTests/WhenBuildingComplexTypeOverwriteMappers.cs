namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingComplexTypeOverwriteMappers
    {
        [Fact]
        public void ShouldBuildASingleSourceSingleTargetMapper()
        {
            var source = new Address { Line1 = "1.1", Line2 = "1.2" };
            var target = new Address { Line1 = "2.1", Line2 = "2.2" };

            GeneratedMapper.Map(source).Over(target);

            target.Line1.ShouldBe("1.1");
            target.Line2.ShouldBe("1.2");
        }

        #region Configuration

        public class ComplexTypeOverwriteMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<Address>().Over<Address>();
            }
        }

        #endregion
    }
}