namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCloningMappers
    {
        [Fact]
        public void ShouldCloneAConfiguration()
        {
            using (var baseMapper = Mapper.CreateNew())
            {
                baseMapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .ToANew<PublicTwoFields<int, int>>()
                    .Map((s, t) => s.Value1 * 2)
                    .To(t => t.Value1);

                using (var childMapper1 = baseMapper.CloneSelf())
                using (var childMapper2 = baseMapper.CloneSelf())
                {
                    childMapper1.WhenMapping
                        .From<PublicTwoFields<int, int>>()
                        .ToANew<PublicTwoFields<int, int>>()
                        .Map((s, t) => s.Value2 + 1)
                        .To(t => t.Value2);

                    childMapper2.WhenMapping
                        .From<PublicTwoFields<int, int>>()
                        .ToANew<PublicTwoFields<int, int>>()
                        .Map((s, t) => s.Value2 + 2)
                        .To(t => t.Value2);

                    var source = new PublicTwoFields<int, int>
                    {
                        Value1 = 4,
                        Value2 = 8
                    };

                    var result1 = childMapper1.Map(source).ToANew<PublicTwoFields<int, int>>();
                    var result2 = childMapper2.Map(source).ToANew<PublicTwoFields<int, int>>();

                    result1.Value1.ShouldBe(8);
                    result1.Value2.ShouldBe(9);

                    result2.Value1.ShouldBe(8);
                    result2.Value2.ShouldBe(10);
                }
            }
        }

        [Fact]
        public void ShouldDifferentiateClonedCloneDataSources()
        {
            using (var parentMapper = Mapper.CreateNew())
            {
                const int SOURCE_VALUE = 3;
                const int ORIGINAL_TARGET_VALUE = 2;
                const int PARENT_INCREMENT = 1;
                const int CHILD_INCREMENT = 2;
                const int GRANDCHILD_INCREMENT = 3;

                var source = new { Value = SOURCE_VALUE };

                parentMapper.WhenMapping
                    .From(source)
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value + ctx.Target.Value + PARENT_INCREMENT)
                    .To(pf => pf.Value);

                using (var childMapper = parentMapper.CloneSelf())
                {
                    childMapper.WhenMapping
                        .From(source)
                        .To<PublicField<int>>()
                        .Map(ctx => ctx.Source.Value + ctx.Target.Value + CHILD_INCREMENT)
                        .To(pf => pf.Value);

                    using (var grandChildMapper = childMapper.CloneSelf())
                    {
                        grandChildMapper.WhenMapping
                            .From(source)
                            .To<PublicField<int>>()
                            .Map(ctx => ctx.Source.Value + ctx.Target.Value + GRANDCHILD_INCREMENT)
                            .To(pf => pf.Value);

                        var target = new PublicField<int> { Value = ORIGINAL_TARGET_VALUE };

                        parentMapper.Map(source).Over(target);

                        const int PARENT_MAP_RESULT =
                            SOURCE_VALUE + ORIGINAL_TARGET_VALUE + PARENT_INCREMENT;

                        target.Value.ShouldBe(PARENT_MAP_RESULT);

                        childMapper.Map(source).Over(target);

                        const int CHILD_MAP_RESULT =
                            SOURCE_VALUE + PARENT_MAP_RESULT + CHILD_INCREMENT;

                        target.Value.ShouldBe(CHILD_MAP_RESULT);

                        const int GRANDCHILD_MAP_RESULT =
                            SOURCE_VALUE + CHILD_MAP_RESULT + GRANDCHILD_INCREMENT;

                        grandChildMapper.Map(source).Over(target);

                        target.Value.ShouldBe(GRANDCHILD_MAP_RESULT);
                    }
                }
            }
        }

        [Fact]
        public void ShouldConditionallyDifferentiateDataSources()
        {
            using (var parentMapper = Mapper.CreateNew())
            {
                parentMapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(pp => pp.Value);

                using (var clonedMapper = parentMapper.CloneSelf())
                {
                    clonedMapper.WhenMapping
                        .From<PublicField<int>>()
                        .To<PublicProperty<string>>()
                        .If((pf, pp) => pf.Value > 5)
                        .Map(ctx => ctx.Source.Value * 3)
                        .To(pp => pp.Value);

                    var source1 = new PublicField<int> { Value = 3 };
                    var source2 = new PublicField<int> { Value = 6 };
                    var target = new PublicProperty<string>();

                    clonedMapper.Map(source1).Over(target);

                    target.Value.ShouldBe("6");

                    clonedMapper.Map(source2).Over(target);

                    target.Value.ShouldBe("18");
                }
            }
        }

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
        public void ShouldConditionallyIgnoreAParentIgnoredMember()
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
        public void ShouldErrorIfClonedMapperHasConflictingDataSourceConfigured()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .From<Person>()
                        .To<PublicField<string>>()
                        .Map((p, pf) => p.Id)
                        .To(pf => pf.Value);

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        try
                        {
                            clonedMapper.WhenMapping
                                .From<Person>()
                                .To<PublicField<string>>()
                                .Map((p, pf) => p.Id + "!")
                                .To(pf => pf.Value);
                        }
                        catch (MappingConfigurationException ex)
                        {
                            ex.ShouldBeNull("Configuring first cloned mapper data source failed");
                        }

                        clonedMapper.WhenMapping
                            .From<Person>()
                            .To<PublicField<string>>()
                            .Map((p, pf) => p.Id + "!!")
                            .To(pf => pf.Value);
                    }
                }
            });
        }

        [Fact]
        public void ShouldErrorIfRedundantDataSourceIsConfigured()
        {
            var conflictEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .From<Address>()
                        .ToANew<Address>()
                        .Map(ctx => ctx.Source.Line1)
                        .To(a => a.Line2);

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .From<Address>()
                            .ToANew<Address>()
                            .Map(ctx => ctx.Source.Line1)
                            .To(a => a.Line2);
                    }
                }
            });

            conflictEx.Message.ShouldContain("already has that configured data source");
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
    }
}
