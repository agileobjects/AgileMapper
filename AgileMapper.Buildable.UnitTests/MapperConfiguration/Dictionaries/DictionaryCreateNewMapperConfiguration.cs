namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration.Dictionaries
{
    using System.Collections.Generic;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class DictionaryCreateNewMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<PublicTwoFields<int, Address>>().ToANew<Dictionary<string, string>>();
        }
    }
}