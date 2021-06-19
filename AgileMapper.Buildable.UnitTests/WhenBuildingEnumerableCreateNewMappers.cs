namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingEnumerableCreateNewMappers
    {
        [Fact]
        public void ShouldBuildASimpleTypeListToCollectionMapper()
        {
            var source = new List<string> { "3", "2", "1", "12345" };
            var result = GeneratedMapper.Map(source).ToANew<Collection<byte?>>();

            result.ShouldNotBeNull();
            result.ShouldBe<byte?>(3, 2, 1, null);
        }

        [Fact]
        public void ShouldBuildASimpleTypeArrayToReadOnlyCollectionMapper()
        {
            var source = new[] { 1, 2, 3 };
            var result = GeneratedMapper.Map(source).ToANew<ReadOnlyCollection<int>>();

            result.ShouldNotBeNull();
            result.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldBuildASimpleTypeHashSetToArrayMapper()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(+1);
            var yesterday = today.AddDays(-1);

            var source = new HashSet<DateTime> { yesterday, today, tomorrow };
            var result = GeneratedMapper.Map(source).ToANew<DateTime[]>();

            result.ShouldNotBeNull().ShouldBe(yesterday, today, tomorrow);
        }

        [Fact]
        public void ShouldBuildAComplexTypeListToIListMapper()
        {
            var source = new List<ProductDto>
            {
                new ProductDto { ProductId = "Surprise" },
                null,
                new ProductDto { ProductId = "Boomstick" }
            };

            var result = GeneratedMapper.Map(source).ToANew<List<ProductDto>>();

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);
            result.First().ShouldNotBeNull().ProductId.ShouldBe("Surprise");
            result.Second().ShouldBeNull();
            result.Third().ShouldNotBeNull().ProductId.ShouldBe("Boomstick");
        }

        #region Configuration

        public class EnumerableCreateNewMapperConfiguration : BuildableMapperConfiguration
        {
            protected override void Configure()
            {
                GetPlanFor<List<string>>().ToANew<Collection<byte?>>();

                GetPlanFor<int[]>().ToANew<ReadOnlyCollection<int>>();

                GetPlanFor<HashSet<DateTime>>().ToANew<DateTime[]>();

                GetPlanFor<List<ProductDto>>().ToANew<IList<ProductDto>>();
            }
        }

        #endregion
    }
}
