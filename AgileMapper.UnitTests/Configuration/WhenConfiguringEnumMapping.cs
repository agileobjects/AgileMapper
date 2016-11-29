namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using TestClasses;
    using Xunit;

    public class WhenConfiguringEnumMapping
    {
        [Fact]
        public void ShouldPairEnumMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                var source = new PublicTwoFields<PaymentTypeUk, PaymentTypeUs>
                {
                    Value1 = PaymentTypeUk.Cheque,
                    Value2 = PaymentTypeUs.Check
                };
                var result = mapper.Map(source).ToANew<PublicTwoParamCtor<PaymentTypeUs, PaymentTypeUk>>();

                result.Value1.ShouldBe(PaymentTypeUs.Check);
                result.Value2.ShouldBe(PaymentTypeUk.Cheque);
            }
        }
    }
}
