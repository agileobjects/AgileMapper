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
    public class WhenConfiguringEnumMappingInline
    {
        [Fact]
        public void ShouldPairEnumMembersInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new PublicTwoFields<PaymentTypeUs, PaymentTypeUk>
                {
                    Value1 = PaymentTypeUs.Check,
                    Value2 = PaymentTypeUk.Cheque
                };

                var result = mapper
                    .Map(source)
                    .ToANew<PublicTwoParamCtor<PaymentTypeUs, PaymentTypeUk>>(cfg => cfg
                        .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check)
                        .And
                        .Map(ctx => ctx.Source.Value1)
                        .ToCtor<PaymentTypeUk>()
                        .And
                        .Map(ctx => ctx.Source.Value2)
                        .ToCtor<PaymentTypeUs>());

                result.Value1.ShouldBe(PaymentTypeUs.Check);
                result.Value2.ShouldBe(PaymentTypeUk.Cheque);
            }
        }
    }
}
