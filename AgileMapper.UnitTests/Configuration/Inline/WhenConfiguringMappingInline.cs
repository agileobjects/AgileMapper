namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Members;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringMappingInline
    {
        [Fact]
        public void ShouldAllowInlineDataSourceExpressionConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            var result1 = Mapper
                .Map(source1)
                .ToANew<PublicField<int>>(c => c
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(pf => pf.Value));


            var result2 = Mapper
                .Map(source2)
                .ToANew<PublicField<int>>(c => c
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(pf => pf.Value));

            result1.Value.ShouldBe(source1.Value * 2);
            result2.Value.ShouldBe(source2.Value * 2);
        }

        [Fact]
        public void ShouldAllowInlineConstantDataSourceExpressionConfig()
        {
            var source1 = new PublicProperty<string> { Value = "Yes" };
            var source2 = new PublicProperty<string> { Value = "No" };

            var result1 = Mapper
                .Map(source1)
                .ToANew<PublicField<string>>(c => c
                    .Map("Maybe?")
                    .To(pf => pf.Value));

            var result2 = Mapper
                .Map(source2)
                .ToANew<PublicField<string>>(c => c
                    .Map("Maybe?")
                    .To(pf => pf.Value));

            result1.Value.ShouldBe("Maybe?");
            result2.Value.ShouldBe("Maybe?");
        }

        [Fact]
        public void ShouldAllowDifferingInlineDelegateDataSourceConfig()
        {
            var source1 = new PublicProperty<int> { Value = 1 };
            var source2 = new PublicProperty<int> { Value = 2 };

            var result1 = Mapper
                .Map(source1)
                .ToANew<PublicField<int>>(c => c
                    .Map(SubtractOne)
                    .To(pf => pf.Value));


            var result2 = Mapper
                .Map(source2)
                .ToANew<PublicField<int>>(c => c
                    .Map(SubtractThree)
                    .To(pf => pf.Value));

            result1.Value.ShouldBe(source1.Value - 1);
            result2.Value.ShouldBe(source2.Value - 3);
        }

        [Fact]
        public void ShouldAllowDifferingInlineConstantDataSourceConfig()
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

        #region Helper Members

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractOne =>
            ctx => ctx.Source.Value - 1;

        private static Expression<Func<IMappingData<PublicProperty<int>, PublicField<int>>, object>> SubtractThree =>
            ctx => ctx.Source.Value - 3;

        #endregion
    }
}
