namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TestClasses;
    using Xunit;

    public class WhenMappingDerivedTypes
    {
        [Fact]
        public void ShouldMapARootComplexTypeFromItsAssignedType()
        {
            object source = new Product { Price = 100.00 };
            var result = Mapper.Map(source).ToANew<Product>();

            result.Price.ShouldBe(100.00);
        }

        [Fact]
        public void ShouldMapARootComplexTypeEnumerableFromItsAssignedType()
        {
            object source = new[] { new Product { Price = 10.01 } };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(10.01);
        }

        [Fact]
        public void ShouldMapARootComplexTypeEnumerableElementFromItsAssignedType()
        {
            var source = new object[] { new Product { Price = 9.99 } };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(9.99);
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberFromItsAssignedType()
        {
            var source = new PublicProperty<object>
            {
                Value = new { Name = "Frank", Address = (object)new Address { Line1 = "Here!" } }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<PersonViewModel>>();

            result.Value.ShouldNotBeNull();
            result.Value.Name.ShouldBe("Frank");
            result.Value.AddressLine1.ShouldBe("Here!");
        }

        [Fact]
        public void ShouldMapAComplexTypeMemberInACollectionFromItsAssignedType()
        {
            var sourceObjectId = Guid.NewGuid();

            var source = new object[]
            {
                new { Name = "Bob", Address = new { Line1 = "There!" } },
                new { Id = sourceObjectId.ToString(), Address = (object)new Address { Line1 = "Somewhere!" } }
            };

            var result = Mapper.Map(source).ToANew<ICollection<PersonViewModel>>();

            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);

            result.First().Id.ShouldBeDefault();
            result.First().Name.ShouldBe("Bob");
            result.First().AddressLine1.ShouldBe("There!");

            result.Second().Id.ShouldBe(sourceObjectId);
            result.Second().Name.ShouldBeNull();
            result.Second().AddressLine1.ShouldBe("Somewhere!");
        }

        [Fact]
        public void ShouldMapAComplexTypeEnumerableMemberFromItsSourceType()
        {
            var source = new PublicField<object>
            {
                Value = new Collection<object>
                {
                    new Customer { Name = "Fred" },
                    new Person { Name = "Bob" }
                }
            };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<object>>();

            var resultValues = ((IEnumerable)result.Value).Cast<Person>().ToArray();
            resultValues.First().ShouldBeOfType<Customer>();
            resultValues.Second().ShouldBeOfType<Person>();
        }

        [Fact]
        public void ShouldMapDerivedTypesFromNestedMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .Map<MegaProduct>()
                    .To<ProductDtoMega>();

                var personSource = new PublicField<Product>
                {
                    Value = new MegaProduct { HowMega = 1.10m }
                };
                var result = mapper.Map(personSource).ToANew<PublicSetMethod<ProductDto>>();

                result.Value.ShouldBeOfType<ProductDtoMega>();
                ((ProductDtoMega)result.Value).HowMega.ShouldBe("1.10");
            }
        }

        [Fact]
        public void ShouldConditionallyMapDerivedTypesFromNestedMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerViewModel>()
                    .If((s, t) => s.Name == "Mystery Customer")
                    .MapTo<MysteryCustomerViewModel>()
                    .And
                    .If((s, t) => s.Name == "Customer Mystery!")
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new PublicField<PersonViewModel>
                {
                    Value = new CustomerViewModel { Name = "Mystery Customer", Discount = 0.5 }
                };
                var result = mapper.Map(mysteryCustomerSource).ToANew<PublicProperty<PersonViewModel>>();

                result.Value.ShouldBeOfType<MysteryCustomerViewModel>();
                ((MysteryCustomerViewModel)result.Value).Discount.ShouldBe(0.5);

                var customerSource = new PublicField<PersonViewModel>
                {
                    Value = new CustomerViewModel { Name = "Banksy" }
                };
                result = mapper.Map(customerSource).ToANew<PublicProperty<PersonViewModel>>();

                result.Value.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateARootDerivedTargetFromADerivedSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Person customer = new Customer();
                var viewModelResult = mapper.Map(customer).ToANew<PersonViewModel>();

                viewModelResult.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateAMemberDerivedTargetFromADerivedSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new PublicProperty<Person> { Value = new Customer() };
                var target = new PublicSetMethod<PersonViewModel>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBeOfType<CustomerViewModel>();
            }
        }

        [Fact]
        public void ShouldCreateAMemberGrandChildDerivedTargetFromADerivedSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var customerSource = new PublicProperty<PersonViewModel>
                {
                    Value = new CustomerViewModel { Discount = 0.5 }
                };

                var customerResult = mapper.Map(customerSource).ToANew<PublicField<Person>>();

                customerResult.Value.ShouldBeOfType<Customer>();
                ((Customer)customerResult.Value).Discount.ShouldBe(0.5);

                var mysteryCustomerSource = new PublicProperty<PersonViewModel>
                {
                    Value = new MysteryCustomerViewModel { Report = "Great!" }
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PublicField<Customer>>();

                mysteryCustomerResult.Value.ShouldBeOfType<MysteryCustomer>();
                ((MysteryCustomer)mysteryCustomerResult.Value).Report.ShouldBe("Great!");
            }
        }

        [Fact]
        public void ShouldCreateARootEnumerableDerivedTargetElementFromADerivedSourceElement()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new[] { new PersonViewModel(), new CustomerViewModel() };
                var result = mapper.Map(source).ToANew<ICollection<Person>>();

                result.First().ShouldBeOfType<Person>();
                result.Second().ShouldBeOfType<Customer>();
            }
        }

        [Fact]
        public void ShouldCreateADerivedTypeInAMemberEnumerableUsingRuntimeTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new PublicProperty<object>
                {
                    Value = new Collection<object>
                    {
                        new CustomerViewModel { Name = "Fred" },
                        new PersonViewModel { Name = "Bob" }
                    }
                };
                var target = new PublicSetMethod<IEnumerable<Person>>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.First().ShouldBeOfType<Customer>();
                result.Value.Second().ShouldBeOfType<Person>();
            }
        }

        [Fact]
        public void ShouldCreateADerivedTypeInAnExistingMemberEnumerableUsingRuntimeTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var personId = Guid.NewGuid();

                var source = new PublicGetMethod<object>(new List<PersonViewModel>
                {
                    new PersonViewModel { Id = personId, Name = "Bob" },
                    new CustomerViewModel { Name = "Fred" }
                });
                var target = new PublicField<object>
                {
                    Value = new Collection<Person>
                    {
                        new Person { Id = personId }
                    }
                };
                var result = mapper.Map(source).OnTo(target);

                var resultValues = (Collection<Person>)result.Value;
                resultValues.Count.ShouldBe(2);

                resultValues.First().ShouldBeOfType<Person>();
                resultValues.First().Id.ShouldBe(personId);
                resultValues.First().Name.ShouldBe("Bob");

                resultValues.Second().ShouldBeOfType<Customer>();
                resultValues.Second().Name.ShouldBe("Fred");
            }
        }

        [Fact]
        public void ShouldMapADerivedTypeToAStruct()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .ToANew<PublicPropertyStruct<string>>()
                    .Map((mc, pps) => mc.Name)
                    .To(pps => pps.Value);

                Customer customer = new MysteryCustomer { Id = Guid.NewGuid(), Name = "Mystery!" };
                var customerResult = mapper.Map(customer).ToANew<PublicPropertyStruct<string>>();

                customerResult.Value.ShouldBe("Mystery!");
            }

            var source = new MysteryCustomer { Discount = 0.2m };
            var result = Mapper.Map(source).ToANew<PersonViewModel>();

            result.ShouldBeOfType<MysteryCustomerViewModel>();
            ((MysteryCustomerViewModel)result).Discount.ShouldBe(0.2);
        }

        [Fact]
        public void ShouldMapDerivedTypesToTheSameTargetType()
        {
            var source = new MysteryCustomer { Id = Guid.NewGuid(), Name = "Customer!" };
            var result = Mapper.Map(source).ToANew<PersonDto>();

            result.Id.ShouldBe(source.Id.ToString());
            result.Name.ShouldBe("Customer!");
        }

        [Fact]
        public void ShouldCreateADerivedTypeForARequestedParentType()
        {
            var source = new MysteryCustomer { Discount = 0.2m };
            var result = Mapper.Map(source).ToANew<PersonViewModel>();

            result.ShouldBeOfType<MysteryCustomerViewModel>();
            ((MysteryCustomerViewModel)result).Discount.ShouldBe(0.2);
        }

        [Fact]
        public void ShouldUseTheDerivedRuntimeTypeOfAnExistingObject()
        {
            Customer source = new MysteryCustomer { Discount = 0.1m, Report = "Yummy" };
            PersonViewModel target = new CustomerViewModel();
            var result = Mapper.Map(source).OnTo(target);

            ((CustomerViewModel)result).Discount.ShouldBe(0.1);
        }

        [Fact]
        public void ShouldUseRuntimeSourceTypeToCreateADerivedTypeForARequestedParentType()
        {
            Customer source = new MysteryCustomer { Discount = 0.333m };
            var result = Mapper.Map(source).ToANew<PersonViewModel>();

            result.ShouldBeOfType<MysteryCustomerViewModel>();
            ((MysteryCustomerViewModel)result).Discount.ShouldBe(0.333);
        }

        [Fact]
        public void ShouldMapMultipleRuntimeTypedChildMembers()
        {
            var source = new PublicTwoFields<IEnumerable<object>, object[]>
            {
                Value1 = new[] { new Product { ProductId = "Giant Trousers", Price = 100.00 } },
                Value2 = new object[] { new MysteryCustomerViewModel { Name = "Mr Faff" } }
            };
            var target = new PublicTwoFields<List<MegaProduct>, ICollection<Customer>>
            {
                Value1 = new List<MegaProduct>(),
                Value2 = Enumerable<Customer>.EmptyArray
            };
            var result = Mapper.Map(source).OnTo(target);

            result.Value1.ShouldHaveSingleItem();
            result.Value1.First().ProductId.ShouldBe("Giant Trousers");
            result.Value1.First().Price.ShouldBe(100.00);

            result.Value2.ShouldHaveSingleItem();
            result.Value2.First().Name.ShouldBe("Mr Faff");
        }

        #region Helper Classes

        // ReSharper disable once ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class PersonDto
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion
    }
}
