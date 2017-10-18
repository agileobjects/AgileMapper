namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Members;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSourcesInline
    {
        [Fact]
        public void ShouldSupportInlineDataSourceExpressionConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value * 2)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value * 2);
                result2.Value.ShouldBe(source2.Value * 2);
            }
        }

        [Fact]
        public void ShouldSupportMultipleInlineDataSourceExpressionConfig()
        {
            var source1 = new PublicProperty<int> { Value = 2 };
            var source2 = new PublicProperty<int> { Value = 4 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicTwoFieldsStruct<int, long>>(
                        c => c.Map(ctx => ctx.Source.Value - 2)
                              .To(pfs => pfs.Value1),
                        c => c.Map(ctx => (long)(ctx.Source.Value / 2))
                              .To(pfs => pfs.Value2));

                result1.Value1.ShouldBe(source1.Value - 2);
                result1.Value2.ShouldBe(source1.Value / 2);

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicTwoFieldsStruct<int, long>>(c => c
                        .Map(ctx => ctx.Source.Value - 2)
                        .To(pfs => pfs.Value1));

                result2.Value1.ShouldBe(source2.Value - 2);
                result2.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldSupportInlineConstantDataSourceExpressionConfig()
        {
            var source1 = new PublicProperty<string> { Value = "Yes" };
            var source2 = new PublicProperty<string> { Value = "No" };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<string>>(c => c
                        .Map("Maybe?")
                        .To(pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<string>>(c => c
                        .Map("Maybe?")
                        .To(pf => pf.Value));

                result1.Value.ShouldBe("Maybe?");
                result2.Value.ShouldBe("Maybe?");
            }

        }

        [Fact]
        public void ShouldSupportDifferingInlineDelegateDataSourceConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(SubtractOne)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(SubtractThree)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value - 1);
                result2.Value.ShouldBe(source2.Value - 3);
            }

        }

        [Fact]
        public void ShouldSupportDifferingInlineConstantDataSourceConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 2)
                        .To(pf => pf.Value));


                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 2);
                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        [Fact]
        public void ShouldSupportDifferingRuleSetInlineDataSourceMemberConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 1);

                var result2 = mapper
                    .Map(source2)
                    .Over(result1, c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        [Fact]
        public void ShouldSupportDifferingTargetTypeInlineDataSourceMemberConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            using (var mapper = Mapper.CreateNew())
            {
                var result1 = mapper
                    .Map(source1)
                    .ToANew<PublicField<int>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                var result2 = mapper
                    .Map(source2)
                    .ToANew<PublicField<long>>(c => c
                        .Map(ctx => ctx.Source.Value + 1)
                        .To(pf => pf.Value));

                result1.Value.ShouldBe(source1.Value + 1);
                result2.Value.ShouldBe(source2.Value + 1);
            }
        }

        #region Helper Members

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractOne =>
            ctx => ctx.Source.Value - 1;

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractThree =>
            ctx => ctx.Source.Value - 3;

        #endregion
    }
}
