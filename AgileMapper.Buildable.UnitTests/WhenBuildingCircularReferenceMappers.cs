namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingCircularReferenceMappers
    {
        [Fact]
        public void ShouldBuildACircularReferenceMapper()
        {
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

            var result = GeneratedMapper.Map(source).ToANew<Child>();

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

        #region Configuration

        public class CircularReferenceMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<Child>().ToANew<Child>();
            }
        }

        #endregion
    }
}