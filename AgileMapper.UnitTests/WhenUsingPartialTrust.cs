namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using Common.TestClasses;
    using TestClasses;
    using Xunit;

    [Trait("Category", "Checked")]
    public class WhenUsingPartialTrust
    {
        [Fact]
        public void ShouldPerformASimpleMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestSimpleMapping());
        }

        [Fact]
        public void ShouldPerformAStringToEnumMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestStringToEnumMapping());
        }

        [Fact]
        public void ShouldPerformADoubleToCharacterMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestDoubleToCharacterMapping());
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
        public void ShouldPerformASourceDictionaryMapping()
        {
            ExecuteInPartialTrust(helper => helper.TestSourceDictionaryMapping());
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
            ExecuteInPartialTrust(helper => helper.TestMappingPlan());
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

                // ReSharper disable once AssignNullToNotNullAttribute
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

        public void TestStringToEnumMapping()
        {
            var source = new PublicProperty<string> { Value = "Mr" };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            Assert.Equal(Title.Mr, result.Value);
        }

        public void TestDoubleToCharacterMapping()
        {
            var source = new PublicProperty<double> { Value = 7 };
            var result = Mapper.Map(source).ToANew<PublicField<char>>();

            Assert.Equal('7', result.Value);
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

        public void TestSourceDictionaryMapping()
        {
            var source = new Dictionary<string, string>
            {
                ["Value.Value"] = "123"
            };
            var result = Mapper.Map(source).ToANew<PublicField<PublicField<int>>>();

            Assert.Equal(123, result.Value.Value);
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
                        .From<PublicTwoFields<string, string>>()
                        .To<PublicField<int>>()
                        .If((s, t) => int.Parse(s.Value1) > 0)
                        .Map(ctx => ctx.Source.Value1)
                        .To(x => x.Value);

                    var source = new PublicTwoFields<string, string> { Value1 = "CantParseThis" };

                    mapper.Map(source).ToANew<PublicField<int>>();
                }
            });
        }

        public void TestMappingPlan()
        {
            string plan = Mapper
                .GetPlanFor<PublicTwoFields<object, object[]>>()
                .Over<PublicTwoFields<Customer, IEnumerable<Customer>>>();

            Assert.Contains(
                "// Map PublicTwoFields<object, object[]> -> PublicTwoFields<Customer, IEnumerable<Customer>>",
                plan);

            Assert.Contains("// Rule Set: Overwrite", plan);
            Assert.Contains("\"Value1\"", plan);
            Assert.Contains("customerICollection.Add((Customer)context.Map(context.AddChild(", RemoveWhiteSpace(plan));
            Assert.Contains("AddElement(objectArray[i],default(Customer),", RemoveWhiteSpace(plan));
        }

        #region Helper Members

        private static string RemoveWhiteSpace(string plan)
            => plan.Replace(Environment.NewLine, null).Replace(" ", null);

        #endregion
    }
}
