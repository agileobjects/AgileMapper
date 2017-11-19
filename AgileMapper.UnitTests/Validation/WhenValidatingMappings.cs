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
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor(new { Thingy = default(string) }).ToANew<PublicProperty<long>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowRightNowIf.MembersAreNotMapped());

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicProperty<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("PublicProperty<long>.Value is unmapped");
            }
        }

        [Fact]
        public void ShouldErrorIfCachedNestedMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor(new
                    {
                        Id = default(string),
                        Title = default(int),
                        Name = default(string),
                        Address = new { Fixxwang = default(string) }
                    })
                    .ToANew<Person>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowRightNowIf.MembersAreNotMapped());

                validationEx.Message.ShouldContain(" -> Person.Address");
                validationEx.Message.ShouldContain("Person.Address.Line1 is unmapped");
                validationEx.Message.ShouldContain("Person.Address.Line2 is unmapped");
            }
        }
    }
}
