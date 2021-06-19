namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

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
}