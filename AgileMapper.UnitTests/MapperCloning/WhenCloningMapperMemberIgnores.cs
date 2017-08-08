namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCloningMapperMemberIgnores
    {
        [Fact]
        public void ShouldIgnoreAParentConfiguredDataSource()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .From<Address>()
                    .ToANew<Address>()
                    .Map((sa, ta) => sa.Line1)
                    .To(ta => ta.Line2);

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .Ignore(ta => ta.Line2);

                    var source = new Address { Line1 = "There there" };

                    var originalResult = originalMapper.Map(source).ToANew<Address>();

                    originalResult.Line2.ShouldBe("There there");

                    var clonedResult = clonedMapper.Map(source).ToANew<Address>();

                    clonedResult.Line1.ShouldBe("There there");
                    clonedResult.Line2.ShouldBeNull();
                }
            }
        }

        [Fact]
        public void ShouldIgnoreAParentIgnoredMemberConditionally()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .From<Address>()
                    .ToANew<Address>()
                    .Ignore(ta => ta.Line2);

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .If(ctx => ctx.Source.Line1 == null)
                        .Ignore(ta => ta.Line2);

                    var source = new Address { Line1 = "There there", Line2 = "Here here" };

                    var originalResult = originalMapper.Map(source).ToANew<Address>();

                    originalResult.Line1.ShouldBe("There there");
                    originalResult.Line2.ShouldBeNull();

                    var clonedResult = clonedMapper.Map(source).ToANew<Address>();

                    clonedResult.Line1.ShouldBe("There there");
                    clonedResult.Line2.ShouldBeNull();

                    source.Line1 = null;

                    var nullLine1ClonedResult = clonedMapper.Map(source).ToANew<Address>();

                    nullLine1ClonedResult.Line1.ShouldBeNull();
                    nullLine1ClonedResult.Line2.ShouldBeNull("Here here");
                }
            }
        }

        [Fact]
        public void ShouldAllowClonedMapperOverlappingMemberFilters()
        {
            using (var originalMapper = Mapper.CreateNew())
            {
                originalMapper.WhenMapping
                    .To<Address>()
                    .IgnoreTargetMembersOfType<string>();

                using (var clonedMapper = originalMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .To<Address>()
                        .IgnoreTargetMembersWhere(member => member.Name == "Line1");
                }
            }
        }

        [Fact]
        public void ShouldErrorIfRedundantIgnoredMemberIsConfigured()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .Ignore(ta => ta.Line2);

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .From<Address>()
                            .ToANew<Address>()
                            .Ignore(ta => ta.Line2);
                    }
                }
            });

            ignoreEx.Message.ShouldContain("has already been ignored");
        }

        [Fact]
        public void ShouldErrorIfFilteredMemberIsIgnored()
        {
            var ignoreEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .ToANew<Address>()
                        .IgnoreTargetMembersWhere(member => member.Name == "Line2");

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .To<Address>()
                            .Ignore(a => a.Line2);
                    }
                }
            });

            ignoreEx.Message.ShouldContain("is already ignored by ignore pattern");
        }
    }
}
