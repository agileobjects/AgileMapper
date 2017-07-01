namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenCloningMappers
    {
        [Fact]
        public void ShouldApplyClonedConfiguration()
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
        public void ShouldSupportDifferentiatingClonedClones()
        {
            using (var parentMapper = Mapper.CreateNew())
            {
                var source = new { Value = 3 };

                parentMapper.WhenMapping
                    .From(source)
                    .To<PublicField<int>>()
                    .Map(ctx => ctx.Source.Value + ctx.Target.Value + 1)
                    .To(pf => pf.Value);

                using (var childMapper = parentMapper.CloneSelf())
                {
                    childMapper.WhenMapping
                        .From(source)
                        .To<PublicField<int>>()
                        .Map(ctx => ctx.Source.Value + ctx.Target.Value + 2)
                        .To(pf => pf.Value);

                    using (var grandChildMapper = childMapper.CloneSelf())
                    {
                        grandChildMapper.WhenMapping
                            .From(source)
                            .To<PublicField<int>>()
                            .Map(ctx => ctx.Source.Value + ctx.Target.Value + 3)
                            .To(pf => pf.Value);

                        var target = new PublicField<int> { Value = 2 };

                        parentMapper.Map(source).Over(target);

                        target.Value.ShouldBe(3 + 2 + 1);

                        childMapper.Map(source).Over(target);

                        target.Value.ShouldBe(3 + 6 + 2);

                        grandChildMapper.Map(source).Over(target);

                        target.Value.ShouldBe(3 + 11 + 3);
                    }
                }
            }
        }

        [Fact]
        public void ShouldApplyConditionalDifferentiatedConfiguration()
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
                                .Map((p, pf) => p.Id)
                                .To(pf => pf.Value);
                        }
                        catch (MappingConfigurationException ex)
                        {
                            ex.ShouldNotBeNull("Configuring first cloned mapper data source failed");
                        }

                        clonedMapper.WhenMapping
                            .From<Person>()
                            .To<PublicField<string>>()
                            .Map((p, pf) => p.Id + "!")
                            .To(pf => pf.Value);
                    }
                }
            });
        }
    }
}
