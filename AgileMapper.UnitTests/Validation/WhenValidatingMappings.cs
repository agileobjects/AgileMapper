namespace AgileObjects.AgileMapper.UnitTests.Validation
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenValidatingMappings
    {
        [Fact]
        public void ShouldSupportMemberMappingValidation()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                Should.NotThrow(() => mapper.ThrowRightNowIf.MembersAreNotMapped());
            }
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersAreNotMapped()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor(new { Thingy = default(string) }).ToANew<PublicProperty<long>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowRightNowIf.MembersAreNotMapped());

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicProperty<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Target.Value");
            }
        }
    }
}
