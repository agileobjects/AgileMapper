namespace AgileObjects.AgileMapper.UnitTests.NonParallel.Configuration
{
    using System;
    using System.Collections.Generic;
    using TestClasses;
    // ReSharper disable once RedundantUsingDirective
    using Shouldly; // <- this using is required even though ReSharper thinks it isn't
    using Xunit;

    public class WhenConfiguringMappingCallbacks : NonParallelTestsBase
    {
        [Fact]
        public void ShouldExecuteAGlobalPreMappingCallbackViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                var mappedTypes = new List<Type>();

                Mapper
                    .Before
                    .MappingBegins
                    .Call((s, t) => mappedTypes.AddRange(new[] { s.GetType(), t?.GetType() }));

                var source = new PersonViewModel { Name = "Bernie", AddressLine1 = "The White House" };
                var target = new Person { Name = "Hillary" };
                Mapper.Map(source).Over(target);

                mappedTypes.ShouldBe(typeof(PersonViewModel), typeof(Person), typeof(PersonViewModel), null);
            });
        }

        [Fact]
        public void ShouldExecuteAGlobalPostMappingCallbackViaTheStaticApiConditionally()
        {
            TestThenReset(() =>
            {
                var mappedTypes = new List<Type>();

                Mapper
                    .After
                    .MappingEnds
                    .If((s, t) => SourceIsPersonViewModel(s, t))
                    .Call((s, t) => mappedTypes.AddRange(new[] { s.GetType(), t.GetType() }));

                var source = new PersonViewModel { Name = "Hillary" };
                var target = new Person { Name = "Bernie", Address = new Address() };
                Mapper.Map(source).Over(target);

                mappedTypes.ShouldBe(typeof(PersonViewModel), typeof(Address), typeof(PersonViewModel), typeof(Person));
            });
        }

        // ReSharper disable once UnusedParameter.Local
        private static bool SourceIsPersonViewModel(object source, object target)
        {
            return source is PersonViewModel;
        }

        [Fact]
        public void ShouldExecutePreAndPostMappingCallbacksForASpecifiedMemberConditionallyViaTheStaticApi()
        {
            TestThenReset(() =>
            {
                var customersWithDiscounts = new List<Customer>();
                var customersAdded = 0;
                var customersRemoved = 0;

                Mapper
                    .WhenMapping
                    .To<Customer>()
                    .Before
                    .Mapping(c => c.Discount)
                    .If((s, c) => c.Discount > 0)
                    .Call((s, c) =>
                    {
                        customersWithDiscounts.Add(c);
                        ++customersAdded;
                    })
                    .And
                    .After
                    .Mapping(c => c.Discount)
                    .If((s, c) => c.Discount == 0)
                    .Call((s, c) =>
                    {
                        customersWithDiscounts.Remove(c);
                        ++customersRemoved;
                    });

                var customer1 = new Customer { Discount = 0.1m };

                Mapper.Map(new Customer { Discount = 0.2m }).Over(customer1);

                customersWithDiscounts.ShouldBe(new[] { customer1 });
                customersAdded.ShouldBe(1);
                customersRemoved.ShouldBe(0);

                var customer2 = new Customer { Discount = 0.1m };

                Mapper.Map(new Customer { Discount = 0.0m }).Over(customer2);

                customersWithDiscounts.ShouldBe(new[] { customer1 });
                customersAdded.ShouldBe(2);
                customersRemoved.ShouldBe(1);
            });
        }
    }
}
