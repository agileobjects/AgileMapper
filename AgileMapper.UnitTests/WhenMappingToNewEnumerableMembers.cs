namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewEnumerableMembers
    {
        [Fact]
        public void ShouldCreateANewStringList()
        {
            var source = new PublicProperty<List<string>> { Value = new List<string> { "Hello", "There", "You" } };
            var result = Mapper.Map(source).ToANew<PublicField<List<string>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.SequenceEqual(source.Value).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateANewIntArray()
        {
            var source = new PublicField<int[]> { Value = new[] { 9, 8, 7, 6, 5 } };
            var result = Mapper.Map(source).ToANew<PublicField<int[]>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.SequenceEqual(source.Value).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateANewIntEnumerable()
        {
            var source = new PublicField<int[]> { Value = new[] { 9, 8, 7, 6, 5 } };
            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<int>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.SequenceEqual(source.Value).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateANewObjectCollection()
        {
            var source = new PublicField<object[]>
            {
                Value = new object[] { 9, new Person { Name = "Mags" }, string.Empty }
            };
            var result = Mapper.Map(source).ToANew<PublicField<Collection<object>>>();

            result.Value.ShouldNotBeNull();
            result.Value.First().ShouldBe(9);
            result.Value.Second().ShouldBeOfType<Person>();
            ((Person)result.Value.Second()).Name.ShouldBe("Mags");
            result.Value.Third().ShouldBe(string.Empty);
        }

        [Fact]
        public void ShouldCreateANewComplexTypeEnumerable()
        {
            var source = new PublicField<Person[]>
            {
                Value = new[] { new Person { Name = "Jack" }, new Person { Name = "Sparrow" } }
            };

            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<PersonViewModel>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe(source.Value.Select(p => p.Name), pvm => pvm.Name);
        }

        [Fact]
        public void ShouldCreateANewNestedComplexTypeEnumerable()
        {
            var source = new PublicField<IEnumerable<PublicField<IEnumerable<PersonViewModel>>>>
            {
                Value = new[]
                {
                    new PublicField<IEnumerable<PersonViewModel>>
                    {
                        Value = new []
                        {
                            new PersonViewModel { Name = "Jack" },
                            new PersonViewModel { Name = "Sparrow" }
                        }
                    },
                    new PublicField<IEnumerable<PersonViewModel>>
                    {
                        Value = new []
                        {
                            new PersonViewModel { Name = "The" },
                            new PersonViewModel { Name = "Lonely" },
                            new PersonViewModel { Name = "Island" }
                        }
                    }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicField<List<PublicField<Collection<Person>>>>>();

            result.Value.Count.ShouldBe(2);

            result.Value.First().Value.Count.ShouldBe(2);
            result.Value.First().Value.First().Name.ShouldBe("Jack");
            result.Value.First().Value.Second().Name.ShouldBe("Sparrow");

            result.Value.Second().Value.Count.ShouldBe(3);
            result.Value.Second().Value.First().Name.ShouldBe("The");
            result.Value.Second().Value.Second().Name.ShouldBe("Lonely");
            result.Value.Second().Value.Third().Name.ShouldBe("Island");
        }

        [Fact]
        public void ShouldCreateANewEnumerableOfComplexTypeArrays()
        {
            var source = new PublicField<IEnumerable<CustomerViewModel[]>>
            {
                Value = new List<CustomerViewModel[]>
                {
                    new[]
                    {
                        new CustomerViewModel { Name = "Jack" },
                        new CustomerViewModel { Name = "Sparrow" }
                    },
                    new[]
                    {
                        new CustomerViewModel { Name = "Andy" },
                        new CustomerViewModel { Name = "Akiva" },
                        new CustomerViewModel { Name = "Jorma" }
                    }
                }
            };

            var result = Mapper.Map(source).ToANew<PublicField<IList<ICollection<Customer>>>>();

            result.Value.Count.ShouldBe(2);

            result.Value.First().Count.ShouldBe(2);
            result.Value.First().First().Name.ShouldBe("Jack");
            result.Value.First().Second().Name.ShouldBe("Sparrow");

            result.Value.Second().Count.ShouldBe(3);
            result.Value.Second().First().Name.ShouldBe("Andy");
            result.Value.Second().Second().Name.ShouldBe("Akiva");
            result.Value.Second().Third().Name.ShouldBe("Jorma");
        }

        [Fact]
        public void ShouldRetainAnExistingListItem()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper
                    .WhenMapping
                    .ToANew<PublicField<List<int>>>()
                    .After
                    .CreatingTargetInstances
                    .Call((s, pf) => pf.Value = new List<int> { 0 });

                var source = new PublicField<int[]> { Value = new[] { 1, 2, 3 } };
                var result = mapper.Map(source).ToANew<PublicField<List<int>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBe(0, 1, 2, 3);
            }
        }

        [Fact]
        public void ShouldRetainAnExistingCollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var existingCollection = new Collection<string>();

                mapper
                    .WhenMapping
                    .From<PublicProperty<IEnumerable<int?>>>()
                    .To<PublicProperty<Collection<string>>>()
                    .After
                    .CreatingTargetInstances
                    .Call((s, t) => t.Value = existingCollection);

                var source = new PublicProperty<IEnumerable<int?>> { Value = new int?[] { 6, 7, 8 } };
                var result = mapper.Map(source).ToANew<PublicProperty<Collection<string>>>();

                result.Value.ShouldBeSameAs(existingCollection);
                result.Value.ShouldBe("6", "7", "8");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionToAnArray()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int[]>>()
                    .Map(ctx => ctx.Source.Value.Split(':'))
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "8:7:6:5" };
                var result = mapper.Map(source).ToANew<PublicField<int[]>>();

                result.Value.ShouldBe(8, 7, 6, 5);
            }
        }

        [Fact]
        public void ShouldHandleANullSourceMemberInAConfiguredEnumerableSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int[]>>()
                    .Map(ctx => ctx.Source.Value.Split(','))
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = null };
                var result = mapper.Map(source).ToANew<PublicField<int[]>>();

                result.Value.ShouldBeEmpty();
            }
        }

        [Fact]
        public void ShouldHandleANullSourceMemberForAWriteOnlyTarget()
        {
            var source = new PublicField<ICollection<int?>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<decimal[]>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldPopulateANonNullReadOnlyNestedICollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                ICollection<Address> addresses = new List<Address>();

                mapper.CreateAReadOnlyFieldUsing(addresses);

                var source = new PublicField<Address[]>
                {
                    Value = new[]
                    {
                        new Address { Line1 = "Address 1" },
                        new Address { Line1 = "Address 2" }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<ICollection<Address>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(addresses);

                result.Value.First().Line1.ShouldBe("Address 1");
                result.Value.Second().Line1.ShouldBe("Address 2");
            }
        }

        [Fact]
        public void ShouldHandleANonNullReadOnlyNestedReadOnlyICollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                ICollection<Address> addresses = new Address[0];

                mapper.CreateAReadOnlyFieldUsing(addresses);

                var source = new PublicField<Address[]>
                {
                    Value = new[]
                    {
                        new Address { Line1 = "Address One" },
                        new Address { Line1 = "Address Two" }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<ICollection<Address>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(addresses);
                result.Value.ShouldBeEmpty();
            }
        }

        [Fact]
        public void ShouldPopulateANonNullReadOnlyNestedEnumerable()
        {
            using (var mapper = Mapper.CreateNew())
            {
                IEnumerable<Address> addresses = new List<Address>();

                mapper.CreateAReadOnlyFieldUsing(addresses);

                var source = new PublicField<Address[]>
                {
                    Value = new[]
                    {
                        new Address { Line1 = "Address 1 Line 1" },
                        new Address { Line1 = "Address 2 Line 1" }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<IEnumerable<Address>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(addresses);

                result.Value.First().Line1.ShouldBe("Address 1 Line 1");
                result.Value.Second().Line1.ShouldBe("Address 2 Line 1");
            }
        }

        [Fact]
        public void ShouldPopulateANonNullReadOnlyNestedIList()
        {
            using (var mapper = Mapper.CreateNew())
            {
                IList<Address> addresses = new List<Address>();

                mapper.CreateAReadOnlyPropertyUsing(addresses);

                var source = new PublicField<Address[]>
                {
                    Value = new[]
                    {
                        new Address { Line1 = "Address 1! Line 1!" },
                        new Address { Line1 = "Address 2! Line 1!" }
                    }
                };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<IList<Address>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(addresses);

                result.Value.First().Line1.ShouldBe("Address 1! Line 1!");
                result.Value.Second().Line1.ShouldBe("Address 2! Line 1!");
            }
        }

        [Fact]
        public void ShouldCreateAnEmptyCollectionByDefault()
        {
            var source = new PublicProperty<Collection<int>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<int>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldMapCollectionsToNullIfConfiguredGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .MapNullCollectionsToNull();

                var source = new PublicField<Collection<byte>> { Value = null };
                var result = mapper.Map(source).ToANew<PublicSetMethod<IEnumerable<string>>>();

                result.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldMapCollectionsToNullIfConfiguredByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<IEnumerable<int>>>()
                    .To<PublicField<List<string>>>()
                    .MapNullCollectionsToNull();

                var matchingSource = new PublicProperty<IEnumerable<int>> { Value = null };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<List<string>>>();

                matchingResult.Value.ShouldBeNull();

                var nonMatchingSource = new PublicProperty<IEnumerable<long>> { Value = null };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<List<string>>>();

                nonMatchingResult.Value.ShouldBeEmpty();
            }
        }
    }
}
