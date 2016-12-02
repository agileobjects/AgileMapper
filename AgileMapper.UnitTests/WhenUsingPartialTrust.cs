namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using TestClasses;
    using Xunit;

    public class WhenUsingPartialTrust
    {
        [Fact]
        public void ShouldPerformASimpleMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestSimpleMapping());
        }

        [Fact]
        public void ShouldPerformAComplexMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestComplexMapping());
        }

        [Fact]
        public void ShouldPerformADerivedMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestDerivedMapping());
        }

        [Fact]
        public void ShouldPerformARuntimeTypedMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestRuntimeTypedMapping());
        }

        [Fact]
        public void ShouldPerformAComplexMappingWithReflectionPermitted()
        {
            ExecuteInPartialTrust(
                helper => helper.TestComplexMapping(),
                PermitReflection);
        }

        [Fact]
        public void ShouldHandleAMaptimeException()
        {
            var mappingException = ExecuteInPartialTrust(helper => helper.TestMappingException());

            Assert.NotNull(mappingException);
        }

        [Fact]
        public void ShouldCreateAMappingPlan()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestMappingPlan();
            });
        }

        private static void ExecuteInPartialTrust(
            Action<MappingHelper> testAction,
            Action<PermissionSet> permissionsSetup = null)
        {
            ExecuteInPartialTrust(
                helper =>
                {
                    testAction.Invoke(helper);
                    return default(object);
                },
                permissionsSetup);
        }

        private static TResult ExecuteInPartialTrust<TResult>(
            Func<MappingHelper, TResult> testFunc,
            Action<PermissionSet> permissionsSetup = null)
        {
            AppDomain partialTrustDomain = null;

            try
            {
                var evidence = new Evidence();
                evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

                var permissions = new NamedPermissionSet(
                    "PartialTrust",
                    SecurityManager.GetStandardSandbox(evidence));

                permissionsSetup?.Invoke(permissions);

                partialTrustDomain = AppDomain.CreateDomain(
                    "PartialTrust",
                    evidence,
                    new AppDomainSetup { ApplicationBase = "." },
                    permissions);

                var helperType = typeof(MappingHelper);

                var helper = (MappingHelper)partialTrustDomain
                    .CreateInstanceAndUnwrap(helperType.Assembly.FullName, helperType.FullName);

                return testFunc.Invoke(helper);
            }
            finally
            {
                if (partialTrustDomain != null)
                {
                    AppDomain.Unload(partialTrustDomain);
                }
            }
        }

        private static void PermitReflection(PermissionSet permissions)
        {
            permissions.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
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

        public void TestRuntimeTypedMapping()
        {
            var source = new PublicProperty<object>
            {
                Value = new PersonViewModel { Name = "Bob" }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Person>>();

            Assert.Equal("Bob", result.Value.Name);
        }

        public MappingException TestMappingException()
        {
            return Assert.Throws<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<string>>()
                        .To<PublicField<int>>()
                        .If((s, t) => int.Parse(s.Value) > 0)
                        .Map(ctx => ctx.Source.Value)
                        .To(x => x.Value);

                    var source = new PublicProperty<string> { Value = "CantParseThis" };

                    mapper.Map(source).ToANew<PublicField<int>>();
                }
            });
        }

        public void TestMappingPlan()
        {
            var plan = Mapper
                .GetPlanFor<PublicTwoFields<object, object[]>>()
                .Over<PublicTwoFields<Customer, IEnumerable<Customer>>>();

            Assert.Contains(
                "// Map PublicTwoFields<object, object[]> -> PublicTwoFields<Customer, IEnumerable<Customer>>",
                plan);

            Assert.Contains("// Map object -> Customer", plan);
            Assert.Contains("// Map object -> Address", plan);
            Assert.Contains("// Rule Set: Overwrite", plan);
        }
    }
}
