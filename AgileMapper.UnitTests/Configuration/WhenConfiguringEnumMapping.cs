namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Shouldly;
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

        [Fact]
        public void ShouldRemovePairedEnumsFromEnumMismatchWarnings()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                var plan = mapper
                    .GetPlanFor<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .OnTo<PublicTwoFields<PaymentTypeUs, PaymentTypeUk>>();

                plan.ShouldNotContain("WARNING");
            }
        }

        [Fact]
        public void ShouldErrorIfSourceValueIsNotAnEnum()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.PairEnum(DateTime.Today);
                }
            });

            enumMappingEx.Message.ShouldContain("DateTime is not an enum type");
        }

        [Fact]
        public void ShouldErrorIfTargetValueIsNotAnEnum()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .PairEnum(PaymentTypeUk.Card).With(TimeSpan.MaxValue);
                }
            });

            enumMappingEx.Message.ShouldContain("TimeSpan is not an enum type");
        }

        [Fact]
        public void ShouldErrorIfSameEnumTypesSpecified()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .PairEnum(PaymentTypeUk.Card).With(PaymentTypeUk.Cash);
                }
            });

            enumMappingEx.Message.ShouldContain("different enum types");
        }
    }
}
