namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Common;
    using TestClasses;
    using Validation;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenValidatingMappings
    {
        [Fact]
        public void ShouldSupportCachedMapperValidation()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingPlanIsIncomplete());
            }
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor(new { Thingy = default(string) }).ToANew<PublicProperty<long>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicProperty<long>");
                validationEx.Message.ShouldContain("Rule set: CreateNew");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicProperty<long>.Value");
            }
        }

        [Fact]
        public void ShouldErrorIfCachedNestedMappingMembersHaveNoDataSources()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor(new
                    {
                        Id = default(string),
                        Title = default(int),
                        Name = default(string)
                    })
                    .Over<Person>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain(" -> Person");
                validationEx.Message.ShouldNotContain(" -> Person.Address");
                validationEx.Message.ShouldContain("Rule set: Overwrite");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("Person.Address");
                validationEx.Message.ShouldContain("Person.Address.Line1");
                validationEx.Message.ShouldContain("Person.Address.Line2");
            }
        }

        [Fact]
        public void ShouldNotErrorIfCachedMappingMemberIsIgnored()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>().To<PublicField<int>>()
                    .Ignore(pf => pf.Value);

                string plan = mapper.GetPlanFor<PublicProperty<string>>().OnTo<PublicField<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingPlanIsIncomplete());
            }
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedCachedMappingMemberIsIgnored()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From(new { LaLaLa = default(int) }).To<PublicField<int>>()
                    .Ignore(pf => pf.Value);

                mapper.GetPlanFor(new { LaLaLa = default(int) }).OnTo<PublicField<int>>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingPlanIsIncomplete());
            }
        }

        [Fact]
        public void ShouldErrorIfComplexTypeMemberIsUnconstructable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor<PublicField<PublicField<int>>>()
                    .ToANew<PublicProperty<PublicTwoParamCtor<int, int>>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("PublicField<PublicField<int>> -> PublicProperty<PublicTwoParamCtor<int, int>>");
                validationEx.Message.ShouldContain("Unconstructable target Types");
                validationEx.Message.ShouldContain("PublicField<int> -> PublicTwoParamCtor<int, int>");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicProperty<PublicTwoParamCtor<int, int>>.Value");
            }
        }

        [Fact]
        public void ShouldNotErrorIfConstructableComplexTypeMemberHasNoMatchingSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Customer>()
                    .Ignore(c => c.Title, c => c.Address.Line2);

                mapper.GetPlanFor<CustomerViewModel>().ToANew<Customer>();

                Should.NotThrow(() => mapper.ThrowNowIfAnyMappingPlanIsIncomplete());
            }
        }

        [Fact]
        public void ShouldErrorIfEnumerableMemberHasNonEnumerableSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor<PublicField<Address>>()
                    .ToANew<PublicProperty<ICollection<string>>>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("PublicField<Address> -> PublicProperty<ICollection<string>>");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicProperty<ICollection<string>>.Value");
            }
        }

        [Fact]
        public void ShouldErrorIfEnumerableMemberHasUnconstructableElements()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .GetPlanFor<Address[]>()
                    .ToANew<PublicCtor<string>[]>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("Address[] -> PublicCtor<string>[]");
                validationEx.Message.ShouldContain("Unconstructable target Types");
                validationEx.Message.ShouldContain("Address -> PublicCtor<string>");
            }
        }

        [Fact]
        public void ShouldShowMultipleIncompleteCachedMappingPlans()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlansFor<Person>().To<ProductDto>();
                mapper.GetPlansFor<Product>().To<PersonViewModel>();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete());

                validationEx.Message.ShouldContain("Person -> ProductDto");
                validationEx.Message.ShouldNotContain("ProductDto.ProductId"); // <- Because PVM has a PersonId
                validationEx.Message.ShouldContain("ProductDto.Price");

                validationEx.Message.ShouldContain("Product -> PersonViewModel");
                validationEx.Message.ShouldContain("PersonViewModel.Name");
                validationEx.Message.ShouldContain("PersonViewModel.AddressLine1");
            }
        }

        [Fact]
        public void ShouldValidateMappingPlanMemberMappingByDefault()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.GetPlanFor(new { Head = "Spinning" }).ToANew<PublicField<string>>());

                validationEx.Message.ShouldContain("AnonymousType<string> -> PublicField<string>");
                validationEx.Message.ShouldContain("Unmapped target members");
                validationEx.Message.ShouldContain("PublicField<string>.Value");
            }
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedMemberHasConfiguredDataSourceWhenValidatingMappingPlansByDefault()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ThrowIfAnyMappingPlanIsIncomplete()
                    .AndWhenMapping
                    .From<Product>().To<PublicProperty<Guid>>()
                    .Map((p, pp) => p.ProductId)
                    .To(p => p.Value);

                Should.NotThrow(() =>
                    mapper.GetPlansFor<Product>().To<PublicProperty<Guid>>());
            }
        }

        [Fact]
        public void ShouldValidateMappingPlanEnumMatchingByDefault()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                var validationEx = Should.Throw<MappingValidationException>(() =>
                    mapper.Map(new PublicField<PaymentTypeUk>()).ToANew<PublicField<PaymentTypeUs>>());

                validationEx.Message.ShouldContain("PublicField<PaymentTypeUk> -> PublicField<PaymentTypeUs>");
                validationEx.Message.ShouldContain("Unpaired enum values");
                validationEx.Message.ShouldContain("PaymentTypeUk.Cheque matches no PaymentTypeUs");
            }
        }

        [Fact]
        public void ShouldNotErrorIfEnumMismatchesAreAllTargetToSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                Should.NotThrow(() =>
                    mapper.GetPlanFor<PublicField<PaymentTypeUk>>().ToANew<PublicField<PaymentType>>());
            }
        }

        [Fact]
        public void ShouldNotErrorIfEnumValuesArePairedWhenValidatingMappingPlansByDefault()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ThrowIfAnyMappingPlanIsIncomplete()
                    .AndWhenMapping
                    .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                Should.NotThrow(() =>
                    mapper.Map(new PublicField<PaymentTypeUk>()).ToANew<PublicField<PaymentTypeUs>>());
            }
        }
    }
}
