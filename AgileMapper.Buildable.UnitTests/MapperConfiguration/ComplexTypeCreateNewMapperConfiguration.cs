namespace AgileObjects.AgileMapper.Buildable.UnitTests.MapperConfiguration
{
    using AgileMapper.UnitTests.Common.TestClasses;
    using Buildable.Configuration;

    public class ComplexTypeCreateNewMapperConfiguration : BuildableMapperConfiguration
    {
        protected override void Configure()
        {
            GetPlanFor<PublicProperty<string>>().ToANew<PublicField<int>>();
                
            GetPlanFor<PublicField<string>>().ToANew<PublicField<int>>();
            GetPlanFor<PublicField<string>>().ToANew<PublicProperty<string>>();
        }
    }
}