namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using AgileMapper.Extensions;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNewComplexTypeMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new Person
            {
                Address = new Address
                {
                    Line1 = "Over here!"
                }
            };

            var result = Mapper.Map(source).ToANew<Person>();

            result.Address.ShouldNotBeNull();
            result.Address.ShouldNotBe(source.Address);
            result.Address.Line1.ShouldBe("Over here!");
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new Person { Name = "Freddie" };

            var result = Mapper.Map(source).ToANew<Person>();

            result.Name.ShouldBe(source.Name);
            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Hello = "There" };

            var result = Mapper.Map(source).ToANew<Customer>();

            result.Address.ShouldBeNull();
        }

        [Fact]
        public void ShouldUnflattenToNestedProperties()
        {
            var source = new WeddingDto
            {
                BrideName = "Nathalie",
                BrideAddressLine1 = "Somewhere",
                GroomName = "Andy"
            };

            var result = Mapper.Map(source).ToANew<Wedding>();

            result.Bride.ShouldNotBeNull();
            result.Bride.Name.ShouldBe("Nathalie");
            result.Bride.Address.Line1.ShouldBe("Somewhere");
            result.Groom.Name.ShouldBe("Andy");
            result.Groom.Address.Line1.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceForNestedCtorParameter()
        {
            var source = new { Value = new { Hello = "There" } };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<PublicCtor<string>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Name + ", " + ctx.Source.AddressLine1)
                    .To(x => x.Address.Line1);

                var source = new PersonViewModel { Name = "Fred", AddressLine1 = "Lala Land" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.Line1.ShouldBe("Fred, Lala Land");
            }
        }

        [Fact]
        public void ShouldHandleRuntimeTypedNestedMemberMatches()
        {
            var runtimeTypedSource = new
            {
                Na = (object)new { Me = "Harry!" },
                Address = (object)new Address { Line1 = "Line Onnneee" }
            };

            var runtimeTypedResult = Mapper.Map(runtimeTypedSource).ToANew<PersonViewModel>();

            runtimeTypedResult.Name.ShouldBe("Harry!");
            runtimeTypedResult.AddressLine1.ShouldBe("Line Onnneee");

            var halfRuntimeTypedSource = new { Na = (object)new { Me = "Boris!" }, Address = (object)123 };

            var halfRuntimeTypedResult = Mapper.Map(halfRuntimeTypedSource).ToANew<PersonViewModel>();

            halfRuntimeTypedResult.Name.ShouldBe("Boris!");
            halfRuntimeTypedResult.AddressLine1.ShouldBeNull();

            var nonRuntimeTypedSource = new { Na = (object)123, Address = (object)456 };

            var nonRuntimeTypedResult = Mapper.Map(nonRuntimeTypedSource).ToANew<PersonViewModel>();

            nonRuntimeTypedResult.Name.ShouldBeNull();
            nonRuntimeTypedResult.AddressLine1.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnUntypedMemberField()
        {
            var source = new PublicField<Address>
            {
                Value = new Address
                {
                    Line1 = "Over there!"
                }
            };

            var result = Mapper.Map(source).ToANew<PublicField<object>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeOfType<Address>();
            ((Address)result.Value).Line1.ShouldBe("Over there!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/13
        [Fact]
        public void ShouldNotMapAnUntypedMemberFieldWithNoMatchingSourceMember()
        {
            var source = new Address
            {
                Line1 = "Some place"
            };

            var result = Mapper.Map(source).ToANew<PublicField<object>>();

            result.Value.ShouldBeNull();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/22
        [Fact]
        public void ShouldUseBestMatchingSourceMemberWhenCloning()
        {
            var source = new Country
            {
                CurrencyId = 1
            };

            var result = Mapper.DeepClone(source);

            result.Currency.ShouldBeNull();
            result.CurrencyId.ShouldBe(1);
        }

        [Fact]
        public void ShouldUseBestMatchingSourceMemberWhenNotCloning()
        {
            var source = new
            {
                Currency = new { Id = 123 },
                CurrencyId = 456
            };

            var result = Mapper.Map(source).ToANew<Country>();

            result.Currency.ShouldNotBeNull();
            result.Currency.Id.ShouldBe(123);
            result.CurrencyId.ShouldBe(456);
        }

        [Fact]
        public void ShouldMapASourcePropertyToMultipleTargets()
        {
            var source = new { CurrencyId = 1 };

            var result = Mapper.Map(source).ToANew<Country>();

            result.Currency.ShouldNotBeNull();
            result.Currency.Id.ShouldBe(1);
            result.CurrencyId.ShouldBe(1);
        }

        [Fact]
        public void ShouldAccessAParentContextInAStandaloneMapper()
        {
            var source = new PublicProperty<object>
            {
                Value = new PersonViewModel { Name = "Fred" }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Person>>();

            result.Value.Name.ShouldBe("Fred");
        }

        [Fact]
        public void ShouldPopulateANonNullReadOnlyNestedMemberProperty()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var address = new Address();

                mapper.CreateAReadOnlyPropertyUsing(address);

                var source = new PublicField<Address> { Value = new Address { Line1 = "Readonly populated!" } };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<Address>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(address);
                result.Value.Line1.ShouldBe("Readonly populated!");
            }
        }

        [Fact]
        public void ShouldHandleANullReadOnlyNestedMemberProperty()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.CreateAReadOnlyFieldUsing(default(Address));

                var source = new PublicGetMethod<Address>(new Address { Line1 = "Not happening..." });
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<Address>>();

                result.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldMapToANonNullUnconstructableNestedMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var existingValue = new PublicField<string>();

                mapper.WhenMapping
                    .ToANew<PublicField<PublicFactoryMethod<PublicField<string>>>>()
                    .CreateInstancesUsing(data => new PublicField<PublicFactoryMethod<PublicField<string>>>
                    {
                        Value = PublicFactoryMethod<PublicField<string>>.Create(existingValue)
                    });

                var source = new { Value = new { Value = new { Value = "Hello!" } } };
                var result = mapper.Map(source).ToANew<PublicField<PublicFactoryMethod<PublicField<string>>>>();

                result.Value.ShouldNotBeNull();
                result.Value.Value.ShouldNotBeNull();
                result.Value.Value.ShouldBeSameAs(existingValue);
                result.Value.Value.Value.ShouldBeSameAs("Hello!");
            }
        }

        [Fact]
        public void ShouldHandleANullUnconstructableRootTarget()
        {
            var source = new { Value = new { Value = "Goodbye!" } };
            var result = Mapper.Map(source).ToANew<PublicUnconstructable<PublicField<string>>>();

            result.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleANullUnconstructableNestedMember()
        {
            var source = new { Value = new { Value = new { Value = "Goodbye!" } } };
            var result = Mapper.Map(source).ToANew<PublicField<PublicUnconstructable<PublicField<string>>>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleAnUnconstructableRuntimeTypedChildMember()
        {
            var result = Mapper
                .Map(new { Value = (object)new { Test = "Nah" } })
                .ToANew<PublicField<PublicCtor<int>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleRuntimeTypedComplexAndEnumerableChildMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var intArraySource = new PublicTwoFields<object, object>
                {
                    Value1 = new Product { ProductId = "kijerf" },
                    Value2 = new[] { 1, 2, 3 }
                };

                var intArrayResult = mapper
                    .Map(intArraySource)
                    .ToANew<PublicTwoFields<ProductDto, List<int?>>>();

                intArrayResult.ShouldNotBeNull();

                intArrayResult.Value1.ShouldNotBeNull();
                intArrayResult.Value1.ProductId.ShouldBe("kijerf");

                intArrayResult.Value2.ShouldBe(1, 2, 3);

                var stringArraySource = new PublicTwoFields<object, object>
                {
                    Value1 = new Product { ProductId = "kdjhdgs" },
                    Value2 = new[] { "3", "2", "1" }
                };

                var stringArrayResult = mapper
                    .Map(stringArraySource)
                    .ToANew<PublicTwoFields<ProductDto, List<int?>>>();

                stringArrayResult.ShouldNotBeNull();

                stringArrayResult.Value1.ShouldNotBeNull();
                stringArrayResult.Value1.ProductId.ShouldBe("kdjhdgs");

                stringArrayResult.Value2.ShouldBe(3, 2, 1);
            }
        }

        [Fact]
        public void ShouldHandleRuntimeTypedComplexAndEnumerableElementMembers()
        {
            var source = new PublicTwoFields<object, IList<object>>
            {
                Value1 = new Product { ProductId = "kjdfskjnds" },
                Value2 = new List<object>
                {
                    new PublicProperty<string> { Value = "ikjhfeslkjdw" },
                    new PublicField<string> { Value = "ldkjkdhusdiuoji" }
                }
            };

            var result = source.DeepClone();

            result.Value1.ShouldBeOfType<Product>();
            ((Product)result.Value1).ProductId.ShouldBe("kjdfskjnds");

            result.Value2.Count.ShouldBe(2);
            result.Value2.First().ShouldBeOfType<PublicProperty<string>>();
            ((PublicProperty<string>)result.Value2.First()).Value.ShouldBe("ikjhfeslkjdw");
            
            result.Value2.Second().ShouldBeOfType<PublicField<string>>();
            ((PublicField<string>)result.Value2.Second()).Value.ShouldBe("ldkjkdhusdiuoji");
        }

        #region Helper Classes

        private class Country
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Currency Currency { get; set; }

            public int CurrencyId { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Currency
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
        }

        #endregion
    }
}
