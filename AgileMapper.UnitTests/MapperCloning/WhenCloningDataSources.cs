namespace AgileObjects.AgileMapper.UnitTests.MapperCloning
{
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCloningDataSources
    {
        [Fact]
        public void ShouldCloneADataSource()
        {
            using (var baseMapper = Mapper.CreateNew())
            {
                baseMapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .ToANew<PublicTwoFields<int, int>>()
                    .Map((s, t) => s.Value1 * 2)
                    .To(t => t.Value1);

                // Populate the derived types cache so it can be cloned:
                baseMapper.GetPlanFor<Customer>().ToANew<CustomerViewModel>();

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
        public void ShouldDifferentiateCloneDataSources()
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
        public void ShouldDifferentiateCloneDataSourcesConditionally()
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

            conflictEx.Message.ShouldContain("already has configured data source Address.Line1");
        }

        [Fact]
        public void ShouldErrorIfFilteredMemberDataSourceIsConfigured()
        {
            var dataSourceEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var originalMapper = Mapper.CreateNew())
                {
                    originalMapper.WhenMapping
                        .ToANew<Address>()
                        .IgnoreTargetMembersWhere(member => member.Name == "Line2");

                    using (var clonedMapper = originalMapper.CloneSelf())
                    {
                        clonedMapper.WhenMapping
                            .From<Address>()
                            .To<Address>()
                            .Map(ctx => ctx.Source.Line1)
                            .To(a => a.Line2);
                    }
                }
            });

            dataSourceEx.Message.ShouldContain("Address.Line1 -> Address.Line2");
            dataSourceEx.Message.ShouldContain("conflicts with member ignore");
            dataSourceEx.Message.ShouldContain("'member.Name == \"Line2\"'");
        }
    }
}
