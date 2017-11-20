﻿namespace AgileObjects.AgileMapper.UnitTests.Validation
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenValidatingMappings
    {
        [Fact]
        public void ShouldSupportCachedMappingMemberValidation()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingIsIncomplete());
            }
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor(new { Thingy = default(string) }).ToANew<PublicProperty<long>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingIsIncomplete());

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicProperty<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicProperty<long>.Value");
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
                        Name = default(string)
                    })
                    .Over<Person>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingIsIncomplete());

                validationEx.Message.ShouldContain(" -> Person");
                validationEx.Message.ShouldNotContain(" -> Person.Address");
                validationEx.Message.ShouldContain("Rule set: Overwrite");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("Person.Address");
                validationEx.Message.ShouldContain("Person.Address.Line1");
                validationEx.Message.ShouldContain("Person.Address.Line2");
            }
        }

        [Fact]
        public void ShouldNotErrorIfCachedMappingMemberIsIgnored()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>().To<PublicField<int>>()
                    .Ignore(pf => pf.Value);

                mapper.GetPlanFor<PublicProperty<string>>().OnTo<PublicField<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingIsIncomplete());
            }
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedCachedMappingMemberIsIgnored()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { LaLaLa = default(int) }).To<PublicField<int>>()
                    .Ignore(pf => pf.Value);

                mapper.GetPlanFor(new { LaLaLa = default(int) }).OnTo<PublicField<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingIsIncomplete());
            }
        }
    }
}
