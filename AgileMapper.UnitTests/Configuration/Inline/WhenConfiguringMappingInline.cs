namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using TestClasses;
    using Xunit;

    public class WhenConfiguringMappingInline
    {
        [Fact]
        public void ShouldAllowInlineDataSourceExpressionConfigViaStaticApi()
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
        public void ShouldAllowDifferingInlineConstantDataSourceConfigViaInstanceApi()
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
    }
}
