namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class EnumerableOverwriteMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<char[]>().Over<int[]>();

            GetPlanFor<Collection<string>>().Over<List<string>>();

            GetPlanFor<ProductDto[]>().Over<ReadOnlyCollection<Product>>();
        }
    }
}