namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.Members;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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
        public void ShouldUseAConfiguredFactoryWithASimpleSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .ToANew<PublicProperty<PublicCtor<string>>>()
                    .Map(ctx => new PublicCtor<string>(ctx.Source.Value))
                    .To(t => t.Value);

                var source = new PublicField<string> { Value = "Hello!" };
                var result = mapper.Map(source).ToANew<PublicProperty<PublicCtor<string>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBe("Hello!");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryWithAComplexTypeMemberBinding()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .InstancesOf<PublicCtor<Address>>()
                    .CreateUsing(ctx => new PublicCtor<Address>(new Address()) { Value = { Line2 = "Some Street" } });

                var source = new PublicField<PersonViewModel> { Value = new PersonViewModel() };
                var target = new PublicSetMethod<PublicCtor<Address>>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldNotBeNull();
                result.Value.Value.Line2.ShouldBe("Some Street");
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

                // The declared target type doesn't match, but CustomerViewModel is automatically 
                // mapped to Customer because of the matching naming conventions:
                nonMatchingTargetResult.Address.Line2.ShouldBe("Frankie House");

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
        public void ShouldUseConfiguredFactoriesForBaseAndDerivedTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .ToANew<PersonViewModel>()
                    .CreateInstancesUsing(ctx => new PersonViewModel { Name = "Person!" });

                mapper.WhenMapping
                    .ToANew<CustomerViewModel>()
                    .CreateInstancesUsing(ctx => new CustomerViewModel { Name = "Customer!" });

                var source = new { Id = Guid.NewGuid() };

                var personResult = mapper.Map(source).ToANew<PersonViewModel>();
                personResult.Id.ShouldBe(source.Id);
                personResult.Name.ShouldBe("Person!");

                var customerResult = mapper.Map(source).ToANew<CustomerViewModel>();
                customerResult.Id.ShouldBe(source.Id);
                customerResult.Name.ShouldBe("Customer!");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/165
        [Fact]
        public void ShouldUseAConfiguredDateTimeFactory()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue165.Timestamp>()
                    .To<DateTime>()
                    .CreateInstancesUsing(ctx => ctx.Source.ToDateTime());

                var source = new { Value = new Issue165.Timestamp { Seconds = 1000 } };

                var result = mapper.Map(source).ToANew<PublicField<DateTime>>();
                result.ShouldNotBeNull();
                result.Value.ShouldBe(source.Value.ToDateTime());
            }
        }

        [Fact]
        public void ShouldUseAConfiguredDateTimeFactoryInARootList()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue165.Timestamp>()
                    .To<DateTime>()
                    .CreateInstancesUsing(ctx => ctx.Source.ToDateTime());

                var source = new List<Issue165.Timestamp>
                {
                    new Issue165.Timestamp { Seconds = 100 },
                    null,
                    new Issue165.Timestamp { Seconds = 200 }
                };

                var result = mapper.Map(source).ToANew<List<DateTime>>();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/90
        [Fact]
        public void ShouldUseAConfiguredFactoryForAnUnconstructableType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue90.Parent>()
                    .ToANew<Issue90.ParentDto>()
                    .CreateInstancesOf<Issue90.Status>()
                    .Using(ctx => Issue90.Status.GetStatus(ctx.Source.ParentStatusId));

                var source = new Issue90.Parent { ParentStatusId = Issue90.Status.StatusId.Approved };
                var result = mapper.Map(source).ToANew<Issue90.ParentDto>();

                result.ShouldNotBeNull();
                result.ParentStatusId.ShouldBe(Issue90.Status.StatusId.Approved);
                result.ParentStatus.ShouldNotBeNull();
                result.ParentStatus.Id.ShouldBe(Issue90.Status.StatusId.Approved);
                result.ParentStatus.Name.ShouldBe("Approved");
            }
        }

        [Fact]
        public void ShouldPrioritiseCreationMethods()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<ConstructionTester>()
                    .ToANew<ConstructionTester>()
                    .If(ctx => ctx.Source.Value1 < 5)
                    .CreateInstancesUsing(ctx => new ConstructionTester(100, 100))
                    .But
                    .If(ctx => ctx.Source.Value1 < 10)
                    .CreateInstancesUsing(ctx => new ConstructionTester(500, 500));

                var lessThanFiveSource = new ConstructionTester(2);
                var lessThanFiveResult = mapper.Map(lessThanFiveSource).ToANew<ConstructionTester>();

                lessThanFiveResult.Value1.ShouldBe(100);
                lessThanFiveResult.Value2.ShouldBe(100);
                lessThanFiveResult.Address.ShouldBeNull();

                var lessThanTenSource = new ConstructionTester(8);
                var lessThanTenResult = mapper.Map(lessThanTenSource).ToANew<ConstructionTester>();

                lessThanTenResult.Value1.ShouldBe(500);
                lessThanTenResult.Value2.ShouldBe(500);
                lessThanTenResult.Address.ShouldBeNull();

                var addressSource = new ConstructionTester(123, 456, new Address { Line1 = "One!" });
                var addressResult = mapper.Map(addressSource).ToANew<ConstructionTester>();

                addressResult.Value1.ShouldBe(123);
                addressResult.Value2.ShouldBe(456);
                addressResult.Address.ShouldNotBeNull().Line1.ShouldBe("One!");

                var noAddressSource = new ConstructionTester(123, 456);
                var noAddressResult = mapper.Map(noAddressSource).ToANew<ConstructionTester>();

                noAddressResult.Value1.ShouldBe(123);
                noAddressResult.Value2.ShouldBe(456);
                noAddressResult.Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldHandleAnObjectMappingDataCreationException()
        {
            var thrownException = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.After.MappingEnds.Call(Console.WriteLine);

                    var source = new PublicField<Product> { Value = new Product() };

                    mapper.Map(source).ToANew<ExceptionThrower<Product>>();

                }
            });

            thrownException.InnerException.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            thrownException.InnerException.Message.ShouldContain("An exception occurred mapping");
        }

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
            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.Message.ShouldContain("PersonViewModel -> Person.Address");
            mappingEx.InnerException.Message.ShouldContain(Constants.CreateNew);

            mappingEx.InnerException.InnerException.ShouldNotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            mappingEx.InnerException.InnerException.Message.ShouldBe("Can't make an address, sorry");
        }

        #region Helper Classes

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

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ExceptionThrower<T>
        {
            // ReSharper disable once NotAccessedField.Local
            private T _value;

            // ReSharper disable once UnusedMember.Local
            public T Value
            {
                get => throw new NotSupportedException("NO, JUST NO");
                set => _value = value;
            }
        }

        private static class Issue90
        {
            public class Status
            {
                public enum StatusId
                {
                    Unsubmitted = 1,
                    Approved = 2,
                    Withdrawn = 3
                }

                private Status(StatusId id)
                {
                    Id = id;
                    Name = id.ToString();
                }

                public StatusId Id { get; }

                public string Name { get; }

                private static readonly Status _unsubmitted = new Status(StatusId.Unsubmitted);
                private static readonly Status _approved = new Status(StatusId.Approved);
                private static readonly Status _withdrawn = new Status(StatusId.Withdrawn);

                public static Status GetStatus(StatusId statusId)
                {
                    switch (statusId)
                    {
                        case StatusId.Unsubmitted:
                            return _unsubmitted;

                        case StatusId.Approved:
                            return _approved;

                        case StatusId.Withdrawn:
                            return _withdrawn;

                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            public class Parent
            {
                public Status.StatusId ParentStatusId { get; set; }

                // ReSharper disable once UnusedMember.Local
                public Status ParentStatus => Status.GetStatus(ParentStatusId);
            }

            public class ParentDto
            {
                // ReSharper disable once UnusedAutoPropertyAccessor.Local
                public Status.StatusId ParentStatusId { get; set; }

                // ReSharper disable once UnusedAutoPropertyAccessor.Local
                public Status ParentStatus { get; set; }
            }
        }

        private class ConstructionTester
        {
            public ConstructionTester(int value1)
                : this(value1, default(int))
            {
            }

            public ConstructionTester(int value1, int value2)
                : this(value1, value2, default(Address))
            {
            }

            public ConstructionTester(int value1, int value2, Address address)
            {
                Value1 = value1;
                Value2 = value2;
                Address = address;
            }

            // ReSharper disable once UnusedMember.Local
            public static ConstructionTester Create(int value1, int value2)
                => new ConstructionTester(value1, value2);

            // ReSharper disable once UnusedMember.Local
            public static ConstructionTester GetInstance(int value1, int value2, Address address)
                => new ConstructionTester(value1, value2, address);

            public int Value1 { get; }

            public int Value2 { get; }

            public Address Address { get; }
        }

        private static class Issue165
        {
            public class Timestamp
            {
                public double Seconds { get; set; }

                public DateTime ToDateTime()
                    => DateTime.UtcNow.Date.AddSeconds(Seconds);
            }
        }

        #endregion
    }
}
