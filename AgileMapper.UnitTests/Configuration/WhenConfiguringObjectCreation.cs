namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Configuration;
    using AgileMapper.Members;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectCreation
    {
        [Fact]
        public void ShouldUseAConfiguredFactoryForAGivenType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .InstancesOf<Address>()
                    .CreateUsing(ctx => new Address { Line2 = "Some Street" });

                var source = new PersonViewModel { AddressLine1 = "Some House" };
                var target = new Person();
                var result = mapper.Map(source).OnTo(target);

                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("Some House");
                result.Address.Line2.ShouldBe("Some Street");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerCtor>()
                    .CreateInstancesUsing(ctx => new CustomerCtor((decimal)ctx.Source.Discount / 2));

                var nonMatchingSource = new PersonViewModel();
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<CustomerCtor>();

                nonMatchingResult.Discount.ShouldBeDefault();

                var matchingSource = new CustomerViewModel { Discount = 10 };
                var matchingResult = mapper.Map(matchingSource).ToANew<CustomerCtor>();

                matchingResult.Discount.ShouldBe(5);
            }
        }

        [Fact]
        public void ShouldUseAConfiguredParameterlessFactoryFuncForASpecifiedSourceAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Address> addressFactory = () => new Address { Line2 = "Customer House" };

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Address>()
                    .CreateInstancesUsing(addressFactory);

                var nonMatchingSource = new PersonViewModel { AddressLine1 = "Blah" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Customer>();

                nonMatchingResult.Address.Line2.ShouldBeNull();

                var matchingSource = new CustomerViewModel { AddressLine1 = "Meh" };
                var matchingResult = mapper.Map(matchingSource).ToANew<Customer>();

                matchingResult.Address.Line2.ShouldBe("Customer House");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredSingleParameterFactoryFuncForASpecifiedSourceTargetAndObjectType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<IMappingData<CustomerViewModel, Customer>, Address> addressFactory =
                    ctx => new Address { Line2 = ctx.Target.Name + " House" };

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<Customer>()
                    .CreateInstancesOf<Address>()
                    .Using(addressFactory);

                var nonMatchingSource = new PersonViewModel { Name = "Benny", AddressLine1 = "Blah" };
                var matchingSource = new CustomerViewModel { Name = "Frankie", AddressLine1 = "Meh" };

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<Customer>();
                nonMatchingSourceResult.Address.Line2.ShouldBeNull();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<Person>();
                nonMatchingTargetResult.Address.Line2.ShouldBeNull();

                var matchingResult = mapper.Map(matchingSource).ToANew<Customer>();
                matchingResult.Address.Line2.ShouldBe("Frankie House");

                var nestedSource = new PublicProperty<CustomerViewModel>
                {
                    Value = new CustomerViewModel { Name = "Johnny" }
                };
                var nestedTarget = new PublicField<Person>
                {
                    Value = new Customer()
                };
                var nestedResult = mapper.Map(nestedSource).OnTo(nestedTarget);
                nestedResult.Value.Address.Line2.ShouldBe("Johnny House");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetTypeConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<PublicField<List<int>>>()
                    .If((cvm, c) => cvm.Discount > 0)
                    .CreateInstancesUsing(ctx => new PublicField<List<int>>
                    {
                        Value = new List<int> { (int)ctx.Source.Discount }
                    });

                var nonMatchingSource = new CustomerViewModel { Discount = 0 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<List<int>>>();

                nonMatchingResult.Value.ShouldBeNull();

                var matchingSource = new CustomerViewModel { Discount = 10 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<List<int>>>();

                matchingResult.Value.ShouldNotBeNull();
                matchingResult.Value.ShouldHaveSingleItem();
                matchingResult.Value.First().ShouldBe(10);
            }
        }

        [Fact]
        public void ShouldUseConfiguredFactoriesForASpecifiedSourceAndTargetTypeConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<PublicField<ICollection<string>>>()
                    .CreateInstancesUsing(ctx => new PublicField<ICollection<string>>
                    {
                        Value = new Collection<string> { ctx.Source.Name }
                    });

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<PublicField<ICollection<string>>>()
                    .If((cvm, c) => cvm.Discount > 0)
                    .CreateInstancesUsing(ctx => new PublicField<ICollection<string>>
                    {
                        Value = new[] { ctx.Source.Name + $" ({ctx.Source.Discount})" }
                    });

                var unconditionalFactorySource = new CustomerViewModel { Name = "Steve", Discount = 0 };
                var unconditionalFactoryResult = mapper.Map(unconditionalFactorySource).ToANew<PublicField<ICollection<string>>>();

                unconditionalFactoryResult.Value.ShouldNotBeNull();
                unconditionalFactoryResult.Value.ShouldHaveSingleItem();
                unconditionalFactoryResult.Value.First().ShouldBe("Steve");

                var conditionalFactorySource = new CustomerViewModel { Name = "Alex", Discount = 1 };
                var conditionalFactoryResult = mapper.Map(conditionalFactorySource).ToANew<PublicField<ICollection<string>>>();

                conditionalFactoryResult.Value.ShouldNotBeNull();
                conditionalFactoryResult.Value.ShouldHaveSingleItem();
                conditionalFactoryResult.Value.First().ShouldBe("Alex (1)");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredTwoParameterFactoryFuncWithAnObjectInitialiser()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Person, PublicReadOnlyProperty<Address>, PublicReadOnlyProperty<Address>> factory =
                    (p, prop) => new PublicReadOnlyProperty<Address>(new Address())
                    {
                        Value = { Line1 = p.Address.Line1, Line2 = p.Address.Line2 }
                    };

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicReadOnlyProperty<Address>>()
                    .CreateInstancesUsing(factory);

                var source = new Person { Address = new Address { Line1 = "Here", Line2 = "There" } };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<Address>>();

                result.Value.Line1.ShouldBe("Here");
                result.Value.Line2.ShouldBe("There");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredThreeParameterFactoryWithAListInitialiser()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Person, PublicReadOnlyProperty<List<string>>, int?, PublicReadOnlyProperty<List<string>>> factory =
                    (p, prop, i) => new PublicReadOnlyProperty<List<string>>(new List<string>())
                    {
                        Value = { p.Id.ToString(), p.Name, i.GetValueOrDefault().ToString() }
                    };

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicReadOnlyProperty<List<string>>>()
                    .CreateInstancesUsing(factory);

                var source = new Person { Id = Guid.NewGuid(), Name = "Giles" };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<List<string>>>();

                result.Value.ShouldBe(source.Id.ToString(), "Giles", "0");
            }
        }

        // ReSharper disable PossibleNullReferenceException
        [Fact]
        public void ShouldUseAConfiguredFactoryForASpecifiedSourceAndTargetTypeInARootEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Map<CustomerViewModel>()
                    .To<CustomerCtor>();

                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerCtor>()
                    .CreateInstancesUsing(ctx => new CustomerCtor((decimal)ctx.Source.Discount * 3)
                    {
                        Number = ctx.EnumerableIndex + 1
                    });

                var source = new[] { new PersonViewModel(), new CustomerViewModel { Discount = 5 } };
                var result = mapper.Map(source).ToANew<Person[]>();

                var customer = result.Second() as CustomerCtor;
                customer.ShouldNotBeNull();
                customer.Discount.ShouldBe(15);
                customer.Number.ShouldBe(2);
            }
        }
        // ReSharper restore PossibleNullReferenceException

        [Fact]
        public void ShouldWrapAnObjectCreationException()
        {
            Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .To<PublicProperty<List<string>>>()
                        .CreateInstancesUsing(ctx => new PublicProperty<List<string>>()
                        {
                            Value = { ctx.Source.Id.ToString(), ctx.Source.Name }
                        });

                    mapper.Map(new Person()).ToANew<PublicProperty<List<string>>>();
                }
            });
        }

        [Fact]
        public void ShouldIncludeMappingDetailsInANestedObjectCreationException()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<Address> addressFactory = () =>
                    {
                        throw new NotSupportedException("Can't make an address, sorry");
                    };

                    mapper
                        .WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);

                    mapper.Map(new PersonViewModel { AddressLine1 = "My House" }).ToANew<Person>();
                }
            });

            mappingEx.Message.ShouldContain("PersonViewModel -> Person");
            mappingEx.Message.ShouldContain(Constants.CreateNew);

            mappingEx.InnerException.ShouldNotBeNull();
            mappingEx.InnerException.Message.ShouldContain("PersonViewModel -> Person.Address");
            mappingEx.InnerException.Message.ShouldContain(Constants.CreateNew);

            mappingEx.InnerException.InnerException.ShouldNotBeNull();
            mappingEx.InnerException.InnerException.Message.ShouldBe("Can't make an address, sorry");
        }

        [Fact]
        public void ShouldErrorIfSingleParameterObjectFactorySpecifiedWithInvalidParameter()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<DateTime, Address> addressFactory = dt => new Address();

                    mapper
                        .WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfTwoParameterObjectFactorySpecifiedWithInvalidParameters()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<int, string, Address> addressFactory = (i, str) => new Address();

                    mapper
                        .WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
                }
            });
        }

        [Fact]
        public void ShouldErrorIfFourParameterObjectFactorySpecified()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<object, object, int?, Address, Address> addressFactory = (i, str, dt, ts) => new Address();

                    mapper
                        .WhenMapping
                        .InstancesOf<Address>()
                        .CreateUsing(addressFactory);
                }
            });
        }

        private class CustomerCtor : Person
        {
            // ReSharper disable once UnusedMember.Local
            public CustomerCtor()
            {
            }

            public CustomerCtor(decimal discount)
            {
                Discount = discount;
            }

            public decimal Discount { get; }

            public int? Number { get; set; }
        }
    }
}
