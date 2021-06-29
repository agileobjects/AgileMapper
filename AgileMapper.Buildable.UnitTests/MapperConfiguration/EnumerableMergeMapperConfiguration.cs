namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using System.Collections.Generic;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class EnumerableMergeMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<IEnumerable<int>>().OnTo<ICollection<int>>();

            GetPlanFor<decimal[]>().OnTo<HashSet<double>>();

            GetPlanFor<Product[]>().OnTo<IEnumerable<Product>>();
        }
    }
}