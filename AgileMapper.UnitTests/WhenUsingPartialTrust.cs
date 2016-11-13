namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Policy;
    using TestClasses;
    using Xunit;

    public class WhenUsingPartialTrust
    {
        [Fact]
        public void ShouldPerformASimpleMapping()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestSimpleMapping();
            });
        }

        [Fact]
        public void ShouldPerformAComplexMapping()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestComplexMapping();
            });
        }

        [Fact]
        public void ShouldPerformADerivedMapping()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestDerivedMapping();
            });
        }

        [Fact]
        public void ShouldCreateAMappingPlan()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestMappingPlan();
            });
        }

        private static void ExecuteInPartialTrust(Action<MappingHelper> testAction)
        {
            AppDomain partialTrustDomain = null;

            try
            {
                var evidence = new Evidence();
                evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

                var permissions = new NamedPermissionSet(
                    "PartialTrust",
                    SecurityManager.GetStandardSandbox(evidence));

                partialTrustDomain = AppDomain.CreateDomain(
                    "PartialTrust",
                    evidence,
                    new AppDomainSetup { ApplicationBase = "." },
                    permissions);

                var helperType = typeof(MappingHelper);

                var helper = (MappingHelper)partialTrustDomain
                    .CreateInstanceAndUnwrap(helperType.Assembly.FullName, helperType.FullName);

                testAction.Invoke(helper);
            }
            finally
            {
                if (partialTrustDomain != null)
                {
                    AppDomain.Unload(partialTrustDomain);
                }
            }
        }
    }

    public class MappingHelper : MarshalByRefObject
    {
        public void TestSimpleMapping()
        {
            var source = new PublicProperty<string> { Value = "I don't trust you..." };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            Assert.Equal("I don't trust you...", result.Value);
        }

        public void TestComplexMapping()
        {
            var source = new Customer { Name = "Untrusted!", Discount = 0.2m };
            var result = Mapper.Map(source).ToANew<CustomerViewModel>();

            Assert.Equal("Untrusted!", result.Name);
            Assert.Equal(0.2, result.Discount);
        }

        public void TestDerivedMapping()
        {
            Person source = new Customer { Name = "Untrusted Person :(", Discount = 0.1m };
            var result = Mapper.Map(source).ToANew<CustomerViewModel>();

            Assert.Equal("Untrusted Person :(", result.Name);
            Assert.Equal(0.1, result.Discount);
        }

        public void TestMappingPlan()
        {
            var plan = Mapper
                .GetPlanFor<PublicTwoFields<object, object[]>>()
                .Over<PublicTwoFields<Customer, IEnumerable<Customer>>>();
        }
    }
}
