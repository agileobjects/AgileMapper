namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
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
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlanFor<PublicProperty<string>>().ToANew<PublicProperty<int>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        [Fact]
        public void ShouldErrorIfCachedMappingMembersHaveNoDataSources()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlanFor(new { Thingy = default(string) }).ToANew<PublicProperty<long>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("AnonymousType<string> -> PublicProperty<long>");
            validationEx.Message.ShouldContain("Rule set: CreateNew");
            validationEx.Message.ShouldContain("Unmapped target members");
            validationEx.Message.ShouldContain("PublicProperty<long>.Value");
        }

        [Fact]
        public void ShouldErrorIfCachedNestedMappingMembersHaveNoDataSources()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var exampleSource = new
                    {
                        Id = default(string),
                        Title = default(int),
                        Name = default(string)
                    };

                    mapper.GetPlanFor(exampleSource).Over<Person>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain(" -> Person");
            validationEx.Message.ShouldNotContain(" -> Person.Address");
            validationEx.Message.ShouldContain("Rule set: Overwrite");
            validationEx.Message.ShouldContain("Unmapped target members");
            validationEx.Message.ShouldContain("Person.Address");
            validationEx.Message.ShouldContain("Person.Address.Line1");
            validationEx.Message.ShouldContain("Person.Address.Line2");
        }

        [Fact]
        public void ShouldNotErrorIfCachedMappingMemberIsIgnored()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicProperty<string>>().To<PublicField<int>>()
                        .Ignore(pf => pf.Value);

                    // ReSharper disable once UnusedVariable
                    string plan = mapper.GetPlanFor<PublicProperty<string>>().OnTo<PublicField<int>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedCachedMappingMemberIsIgnored()
        {
            Should.NotThrow(() =>
            {
                var exampleSource = new { LaLaLa = default(int) };

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From(exampleSource).To<PublicField<int>>()
                        .Ignore(pf => pf.Value);

                    mapper.GetPlanFor(exampleSource).OnTo<PublicField<int>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        [Fact]
        public void ShouldErrorIfRootComplexTypeIsUnmappable()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlanFor(new { Value = default(int) })
                          .ToANew<PublicUnconstructable<int>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("AnonymousType<int> -> PublicUnconstructable<int>");
            validationEx.Message.ShouldContain("Unmappable target Types");
        }

        [Fact]
        public void ShouldRecogniseConstructableComplexTypeMembersWithNoMappableMembers()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .GetPlanFor<PublicProperty<PublicTwoFieldsStruct<int, int>>>()
                        .ToANew<PublicField<PublicField<int>>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("PublicProperty<PublicTwoFieldsStruct<int, int>> -> PublicField<PublicField<int>>");
            validationEx.Message.ShouldNotContain("Unconstructable target Types");
            validationEx.Message.ShouldNotContain("PublicTwoFieldsStruct<int, int> -> PublicField<int>");
            validationEx.Message.ShouldContain("Unmapped target members");
            validationEx.Message.ShouldContain("- PublicField<PublicField<int>>.Value.Value");
        }

        [Fact]
        public void ShouldNotErrorIfConstructableComplexTypeMemberHasNoMatchingSource()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<CustomerViewModel>()
                        .To<Customer>()
                        .Ignore(c => c.Title, c => c.Address.Line2);

                    mapper.GetPlanFor<CustomerViewModel>().ToANew<Customer>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfUnconstructableComplexTypeMemberHasFactoryMethod()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlanFor<PublicField<string>>().ToANew<UnconstructableFactoryMethod>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfUnconstructableComplexTypeMemberHasConfiguredFactoryMethod()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Address>()
                        .ToANew<PublicCtor<string>>()
                        .CreateInstancesUsing(ctx => new PublicCtor<string>(ctx.Source.Line1));

                    mapper
                        .GetPlanFor<PublicField<Address>>()
                        .ToANew<PublicField<PublicCtor<string>>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });
        }

        // See https://github.com/agileobjects/AgileMapper/issues/183
        [Fact]
        public void ShouldNotErrorIfAbstractMemberHasDiscoverableDerivedTypePairs()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                    mapper.WhenMapping
                        .From<Issue183.ThingBase>().ButNotDerivedTypes
                        .To<Issue183.ThingDto>()
                        .Ignore(t => t.Value);

                    mapper.GetPlanFor<PublicField<Issue183.ThingBase>>()
                          .ToANew<PublicField<Issue183.ThingBaseDto>>();
                }
            });
        }

        [Fact]
        public void ShouldErrorIfEnumerableMemberHasNonEnumerableSource()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .GetPlanFor<PublicField<Address>>()
                        .ToANew<PublicProperty<ICollection<string>>>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("PublicField<Address> -> PublicProperty<ICollection<string>>");
            validationEx.Message.ShouldContain("Unmapped target members");
            validationEx.Message.ShouldContain("PublicProperty<ICollection<string>>.Value");
        }

        [Fact]
        public void ShouldErrorIfEnumerableMemberHasUnmappableElements()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper
                        .GetPlanFor<Address[]>()
                        .ToANew<PublicCtor<string>[]>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("Address[] -> PublicCtor<string>[]");
            validationEx.Message.ShouldContain("Unmappable target Types");
            validationEx.Message.ShouldContain("Address -> PublicCtor<string>");
        }

        [Fact]
        public void ShouldShowMultipleIncompleteCachedMappingPlans()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.GetPlansFor<Person>().To<ProductDto>();
                    mapper.GetPlansFor<Product>().To<PersonViewModel>();

                    mapper.ThrowNowIfAnyMappingPlanIsIncomplete();
                }
            });

            validationEx.Message.ShouldContain("Person -> ProductDto");
            validationEx.Message.ShouldNotContain("ProductDto.ProductId"); // <- Because PVM has a PersonId
            validationEx.Message.ShouldContain("ProductDto.Price");

            validationEx.Message.ShouldContain("Product -> PersonViewModel");
            validationEx.Message.ShouldContain("PersonViewModel.Name");
            validationEx.Message.ShouldContain("PersonViewModel.AddressLine1");
        }

        [Fact]
        public void ShouldValidateMappingPlanMemberMappingByDefault()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                    mapper.GetPlanFor(new { Head = "Spinning" }).ToANew<PublicField<string>>();
                }
            });

            validationEx.Message.ShouldContain("AnonymousType<string> -> PublicField<string>");
            validationEx.Message.ShouldContain("Unmapped target members");
            validationEx.Message.ShouldContain("PublicField<string>.Value");
        }

        [Fact]
        public void ShouldNotErrorIfUnmappedMemberHasConfiguredDataSourceWhenValidatingMappingPlansByDefault()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ThrowIfAnyMappingPlanIsIncomplete()
                        .AndWhenMapping
                        .From<Product>().To<PublicProperty<Guid>>()
                        .Map((p, pp) => p.ProductId)
                        .To(p => p.Value);

                    mapper.GetPlansFor<Product>().To<PublicProperty<Guid>>();
                }
            });
        }

        [Fact]
        public void ShouldValidateMappingPlanEnumMatchingByDefault()
        {
            var validationEx = Should.Throw<MappingValidationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                    mapper.Map(new PublicField<PaymentTypeUk>()).ToANew<PublicField<PaymentTypeUs>>();
                }
            });

            validationEx.Message.ShouldContain("PublicField<PaymentTypeUk> -> PublicField<PaymentTypeUs>");
            validationEx.Message.ShouldContain("Unpaired enum values");
            validationEx.Message.ShouldContain("PaymentTypeUk.Cheque matches no PaymentTypeUs");
        }

        [Fact]
        public void ShouldNotErrorIfEnumMismatchesAreAllTargetToSource()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                    mapper.GetPlanFor<PublicField<PaymentTypeUk>>().ToANew<PublicField<PaymentType>>();
                }
            });
        }

        [Fact]
        public void ShouldNotErrorIfEnumValuesArePairedWhenValidatingMappingPlansByDefault()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ThrowIfAnyMappingPlanIsIncomplete()
                        .AndWhenMapping
                        .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check);

                    mapper.Map(new PublicField<PaymentTypeUk>()).ToANew<PublicField<PaymentTypeUs>>();
                }
            });
        }

        // See https://github.com/agileobjects/AgileMapper/issues/184
        [Fact]
        public void ShouldNotErrorIfMembersAreMatchedByAToTargetDataSource()
        {
            Should.NotThrow(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.ThrowIfAnyMappingPlanIsIncomplete();

                    mapper.WhenMapping
                        .From<PublicField<Address>>()
                        .To<Address>()
                        .Map((pf, _) => pf.Value)
                        .ToTarget();

                    mapper.GetPlanFor<PublicField<Address>>()
                          .ToANew<PublicTwoFields<Address, Address>>();
                }
            });
        }

        #region Helper Classes

        private class UnconstructableFactoryMethod
        {
            private UnconstructableFactoryMethod(string valueString)
            {
                Value = valueString;
            }

            // ReSharper disable once UnusedMember.Local
            public static UnconstructableFactoryMethod Create(string value)
                => new UnconstructableFactoryMethod(value);

            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Value { get; }
        }

        public static class Issue183
        {
            public abstract class ThingBase
            {
            }

            public class Thing : ThingBase
            {
                public string Value { get; set; }
            }

            public abstract class ThingBaseDto
            {
            }

            public class ThingDto : ThingBaseDto
            {
                public string Value { get; set; }
            }
        }

        #endregion
    }
}
