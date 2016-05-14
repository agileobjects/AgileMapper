namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSources
    {
        [Fact]
        public void ShouldApplyAConfiguredConstant()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicProperty<string>>()
                    .Map("Hello there!")
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "Goodbye!" };
                var result = mapper.Map(source).ToNew<PublicProperty<string>>();

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredConstantFromAllSourceTypes()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .To<PublicProperty<decimal>>()
                    .Map(decimal.MaxValue)
                    .To(x => x.Value);

                var mapNewResult1 = mapper.Map(new PublicField<decimal>()).ToNew<PublicProperty<decimal>>();
                var mapNewResult2 = mapper.Map(new Person()).ToNew<PublicProperty<decimal>>();
                var mapNewResult3 = mapper.Map(new PublicGetMethod<float>(1.0f)).ToNew<PublicProperty<decimal>>();

                mapNewResult1.Value.ShouldBe(decimal.MaxValue);
                mapNewResult2.Value.ShouldBe(decimal.MaxValue);
                mapNewResult3.Value.ShouldBe(decimal.MaxValue);
            }
        }

        [Fact]
        public void ShouldConditionallyApplyAConfiguredConstant()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .Over<PublicProperty<string>>()
                    .Map("Too small!")
                    .To(x => x.Value)
                    .If(ctx => ctx.Target.Value.Length < 5);

                var source = new PublicProperty<string> { Value = "Replaced" };

                var nonMatchingTarget = new PublicProperty<string> { Value = "This has more than 5 characters" };
                var nonMatchingResult = mapper.Map(source).Over(nonMatchingTarget);

                nonMatchingResult.Value.ShouldBe("Replaced");

                var matchingTarget = new PublicProperty<string> { Value = "Tiny" };
                var matchingResult = mapper.Map(source).Over(matchingTarget);

                matchingResult.Value.ShouldBe("Too small!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(x => x.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToNew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);
            }
        }

        [Fact]
        public void ShouldConditionallyApplyAConfiguredMember()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicSetMethod<string>>()
                    .Map(ctx => ctx.Source.Value)
                    .To<string>(x => x.SetValue)
                    .If(ctx => ctx.Source.Value % 2 == 0);

                var matchingSource = new PublicField<int> { Value = 6 };
                var matchingResult = mapper.Map(matchingSource).ToNew<PublicSetMethod<string>>();

                matchingResult.Value.ShouldBe("6");

                var nonMatchingSource = new PublicField<int> { Value = 7 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToNew<PublicSetMethod<string>>();

                nonMatchingResult.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredMemberFromAllSourceTypes()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .OnTo<Person>()
                    .Map(x => x.Source.GetType().Name)
                    .To(x => x.Name);

                var personResult = mapper.Map(new Person()).OnTo(new Person());
                var customerResult = mapper.Map(new Customer()).OnTo(new Person());

                personResult.Name.ShouldBe("Person");
                customerResult.Name.ShouldBe("Customer");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredMemberInARootEnumerable()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicField<string>>()
                    .Map(ctx => ctx.Source.Name)
                    .To(x => x.Value);

                var source = new[] { new Person { Name = "Mr Thomas" } };
                var result = mapper.Map(source).ToNew<List<PublicField<string>>>();

                source.ShouldBe(result.Select(r => r.Value), p => p.Name);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredMemberFromADerivedSourceType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map(ctx => ctx.Source.Id)
                    .To(x => x.Name);

                var source = new Customer { Id = Guid.NewGuid(), Address = new Address() };
                var result = mapper.Map(source).ToNew<PersonViewModel>();

                result.Name.ShouldBe(source.Id.ToString());
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicField<long>>()
                    .Map(ctx => ctx.Source.Value * 10)
                    .To(x => x.Value);

                var source = new PublicProperty<int> { Value = 123 };
                var result = mapper.Map(source).ToNew<PublicField<long>>();

                result.Value.ShouldBe(source.Value * 10);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionInAMemberEnumerable()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .To<PublicSetMethod<string>>()
                    .Map((s, t, i) => (i + 1) + ": " + s.Name)
                    .To<string>(x => x.SetValue);

                var source = new PublicProperty<Customer[]> { Value = new[] { new Customer { Name = "Mr Thomas" } } };
                var result = mapper.Map(source).ToNew<PublicField<IEnumerable<PublicSetMethod<string>>>>();

                result.Value.ShouldHaveSingleItem();
                result.Value.First().Value.ShouldBe("1: Mr Thomas");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionWithMultipleNestedSourceMembers()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map((p, pvm) => p.Address.Line1 + ", " + p.Address.Line2)
                    .To(x => x.AddressLine1);

                var source = new Person { Address = new Address { Line1 = "One", Line2 = "Two" } };
                var result = mapper.Map(source).ToNew<PersonViewModel>();

                result.AddressLine1.ShouldBe("One, Two");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionToADerivedTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + "!")
                    .To(x => x.Name);

                var source = new PersonViewModel { Name = "Harry" };
                var result = mapper.Map(source).ToNew<Customer>();

                result.Name.ShouldBe(source.Name + "!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionInARootCollectionConditionally()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<PersonViewModel>()
                    .Map((s, t) => t.Name + $" ({((Customer)s).Discount})")
                    .To(pvm => pvm.Name)
                    .If((s, t) => (s as Customer) != null);

                var customerId = Guid.NewGuid();

                var source = new[]
                {
                    new Customer { Id = customerId, Name = "Mr Thomas", Discount = 0.10m },
                    new Person { Name = "Mrs Edison" }
                };

                var target = new Collection<PersonViewModel>
                {
                    new PersonViewModel { Id = customerId, Name = "Mrs Thomas" }
                };

                var result = mapper.Map(source).Over(target);

                result.Count.ShouldBe(2);
                result.First().Name.ShouldBe("Mrs Thomas (0.10)");
                result.Second().Name.ShouldBe("Mrs Edison");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredFunction()
        {
            using (var mapper = Mapper.Create())
            {
                Func<ITypedMemberMappingContext<Person, PersonViewModel>, string> combineAddressLine1 =
                    ctx => ctx.Source.Name + ", " + ctx.Source.Address.Line1;

                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Map(combineAddressLine1)
                    .To(pvm => pvm.AddressLine1);

                var source = new Person { Name = "Frank", Address = new Address { Line1 = "Over there" } };
                var result = mapper.Map(source).ToNew<PersonViewModel>();

                result.AddressLine1.ShouldBe("Frank, Over there");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredSourceAndTargetFunction()
        {
            using (var mapper = Mapper.Create())
            {
                Func<PersonViewModel, Address, string> combineAddressLine1 =
                    (pvm, a) => pvm.Name + ", " + pvm.AddressLine1;

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Address>()
                    .Map(combineAddressLine1)
                    .To(p => p.Line1);

                var source = new PersonViewModel { Name = "Francis", AddressLine1 = "Over here" };
                var result = mapper.Map(source).ToNew<Person>();

                result.Address.Line1.ShouldBe("Francis, Over here");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredSourceTargetAndIndexFunction()
        {
            using (var mapper = Mapper.Create())
            {
                Func<PersonViewModel, Person, int?, string> combineAddressLine1 =
                    (pvm, p, i) => $"{i}: {pvm.Name}, {pvm.AddressLine1}";

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map(combineAddressLine1)
                    .To(p => p.Address.Line1);

                var source = new[] { new PersonViewModel { Name = "Jane", AddressLine1 = "Over here!" } };
                var result = mapper.Map(source).ToNew<Person[]>();

                result.ShouldHaveSingleItem();
                result.First().Address.Line1.ShouldBe("0: Jane, Over here!");
            }
        }

        [Fact]
        public void ShouldMapAConfiguredFunction()
        {
            using (var mapper = Mapper.Create())
            {
                Func<Person, string> combineAddressLine1 =
                    p => p.Name + ", " + p.Address.Line1;

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Func<Person, string>>>()
                    .MapFunc(combineAddressLine1)
                    .To(x => x.Value);

                var source = new Person { Name = "Frank", Address = new Address { Line1 = "Over there" } };
                var target = mapper.Map(source).Over(new PublicProperty<Func<Person, string>>());

                target.Value.ShouldBe(combineAddressLine1);
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionUsingExtensionMethods()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<Customer>()
                    .Map(ctx => (decimal)ctx.Source.Name.First())
                    .To(x => x.Discount);

                var source = new Person { Name = "Bob" };
                var result = mapper.Map(source).ToNew<Customer>();

                result.Discount.ShouldBe(source.Name.First());
            }
        }

        [Fact]
        public void ShouldRestrictConfiguredConstantApplicationBySourceType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicSetMethod<int>>()
                    .Map(12345)
                    .To<int>(x => x.SetValue);

                var matchingSource = new PublicField<int> { Value = 938726 };
                var matchingSourceResult = mapper.Map(matchingSource).ToNew<PublicSetMethod<int>>();

                var nonMatchingSource = new PublicProperty<int> { Value = matchingSource.Value };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToNew<PublicSetMethod<int>>();

                matchingSourceResult.Value.ShouldBe(12345);
                nonMatchingSourceResult.Value.ShouldBe(nonMatchingSource.Value);
            }
        }

        [Fact]
        public void ShouldRestrictConfiguredConstantApplicationByTargetType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .Map(98765)
                    .To(x => x.Value);

                var source = new PublicField<int> { Value = 938726 };
                var matchingTargetResult = mapper.Map(source).ToNew<PublicProperty<int>>();
                var nonMatchingTargetResult = mapper.Map(source).ToNew<PublicSetMethod<int>>();

                matchingTargetResult.Value.ShouldBe(98765);
                nonMatchingTargetResult.Value.ShouldBe(source.Value);
            }
        }

        [Fact]
        public void ShouldRestrictConfigurationApplicationByMappingMode()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<long>>()
                    .Map(9999)
                    .To(x => x.Value);

                var source = new PublicProperty<int> { Value = 64738 };
                var matchingModeResult = mapper.Map(source).ToNew<PublicProperty<long>>();

                var nonMatchingModeTarget = mapper.Map(source).Over(new PublicProperty<long>());

                matchingModeResult.Value.ShouldBe(9999);
                nonMatchingModeTarget.Value.ShouldBe(source.Value);
            }
        }
    }
}
