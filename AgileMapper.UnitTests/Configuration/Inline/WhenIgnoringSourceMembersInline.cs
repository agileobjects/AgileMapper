namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersInline
    {
        [Fact]
        public void ShouldIgnoreASourceMemberConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingResult = mapper
                    .Map(new CustomerViewModel { Discount = 0.5 })
                    .ToANew<Customer>(cfg => cfg
                        .If(ctx => ctx.Source.Discount > 0.3)
                        .IgnoreSource(cvm => cvm.Discount));

                matchingResult.Discount.ShouldBeDefault();

                var nonMatchingResult = mapper
                    .Map(new CustomerViewModel { Discount = 0.2 })
                    .ToANew<Customer>(cfg => cfg
                        .If(ctx => ctx.Source.Discount > 0.3)
                        .IgnoreSource(cvm => cvm.Discount));

                nonMatchingResult.Discount.ShouldBe(0.2m);

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldExtendSourceMemberIgnoreConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoFields<long, long>>()
                    .If((sptf, tptf) => sptf.Value1 < 5)
                    .IgnoreSource(sptf => sptf.Value1);   // Ignore source.Value1 < 5

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 4, Value2 = 8 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .IgnoreSource(sptf => sptf.Value2)); // Ignore source.Value2 <= 10

                result1.Value1.ShouldBeDefault();
                result1.Value2.ShouldBeDefault();

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 5, Value2 = 7 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .IgnoreSource(sptf => sptf.Value2)); // Ignore source.Value2 <= 10

                result2.Value1.ShouldBe(5);
                result2.Value2.ShouldBeDefault();

                mapper.InlineContexts().ShouldHaveSingleItem();

                var result3 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 5, Value2 = 11 })
                    .OnTo(new PublicTwoFields<long, long>(), c => c
                        .If((sptf, tptf) => sptf.Value1 >= 3)
                        .IgnoreSource(sptf => sptf.Value1) // Ignore source.Value1 >= 3
                        .And
                        .If((sptf, tptf) => sptf.Value2 <= 10)
                        .IgnoreSource(sptf => sptf.Value2)); // Ignore source.Value2 < 10

                result3.Value1.ShouldBeDefault();
                result3.Value2.ShouldBe(11);

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }
    }
}
