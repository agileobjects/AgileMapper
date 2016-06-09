namespace AgileObjects.AgileMapper.UnitTests.Plans
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldIncludeASimpleTypeMemberMapping()
        {
            var plan = Mapper
                .GetPlanFor<PublicField<string>>()
                .ToANew<PublicProperty<string>>();

            plan.ShouldContain("instance.Value = omc.Source.Value;");
        }
    }
}
