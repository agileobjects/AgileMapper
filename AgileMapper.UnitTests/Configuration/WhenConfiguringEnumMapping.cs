namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
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

        // See https://github.com/agileobjects/AgileMapper/issues/138
        [Fact]
        public void ShouldApplyEnumPairsToRootMappings()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                var ukChequeResult = mapper.Map(PaymentTypeUk.Cheque).ToANew<PaymentTypeUs>();

                ukChequeResult.ShouldBe(PaymentTypeUs.Check);

                var usCheckResult = mapper.Map(PaymentTypeUs.Check).ToANew<PaymentTypeUk>();

                usCheckResult.ShouldBe(PaymentTypeUk.Cheque);
            }
        }

        [Fact]
        public void ShouldApplyEnumPairsToNullableRootMappings()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                var ukChequeResult = mapper.Map(PaymentTypeUk.Cheque).ToANew<PaymentTypeUs?>();

                ukChequeResult.ShouldBe(PaymentTypeUs.Check);

                var usCheckResult = mapper.Map((PaymentTypeUs)1234).ToANew<PaymentTypeUk?>();

                usCheckResult.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldAllowMultipleSourceToSingleTargetPairing()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .PairEnums(PaymentTypeUk.Cheque, PaymentTypeUk.Cash).With(PaymentTypeUs.Check)
                    .And
                    .PairEnum(PaymentTypeUs.Check).With(PaymentTypeUk.Cheque);

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
                    .From<PublicTwoFields<PaymentTypeUk, PaymentTypeUs>>()
                    .To<PublicTwoFields<PaymentTypeUs, PaymentTypeUk>>()
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                string plan = mapper
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

        [Fact]
        public void ShouldErrorIfNoSourceEnumMembersSpecified()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .PairEnums<PaymentTypeUk>()
                        .With(PaymentTypeUs.Check);
                }
            });

            enumMappingEx.Message.ShouldContain("Pairing enum members");
        }

        [Fact]
        public void ShouldErrorIfNoTargetEnumMembersSpecified()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .PairEnums(PaymentTypeUk.Cheque)
                        .With<PaymentTypeUs>();
                }
            });

            enumMappingEx.Message.ShouldContain("Paired enum members");
        }

        [Fact]
        public void ShouldErrorIfIncompatibleNumbersOfEnumMembersSpecified()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .PairEnums(PaymentTypeUk.Cheque, PaymentTypeUk.Card, PaymentTypeUk.Cash)
                        .With(PaymentTypeUs.Check, PaymentTypeUs.Cash);
                }
            });

            enumMappingEx.Message.ShouldContain("2 pairing enum values are required");
        }

        [Fact]
        public void ShouldErrorIfSourceEnumMemberConflictingPairSpecified()
        {
            var enumMappingEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);
                    mapper.WhenMapping.PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Card);
                }
            });

            enumMappingEx.Message.ShouldContain("Cheque is already paired with PaymentTypeUs.Check");
        }
    }
}
