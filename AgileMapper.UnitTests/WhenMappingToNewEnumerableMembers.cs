namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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
            result.Value.AsEnumerable().ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldCreateANewIntEnumerable()
        {
            var source = new PublicField<int[]> { Value = new[] { 9, 8, 7, 6, 5 } };
            var result = Mapper.Map(source).ToANew<PublicField<IEnumerable<int>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.ShouldBe(source.Value);
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
        public void ShouldCreateANewObjectReadOnlyCollection()
        {
            var source = new PublicField<object[]>
            {
                Value = new object[] { 9, new CustomerViewModel { Name = "Boycee" }, default(string) }
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<ReadOnlyCollection<object>>>();

            result.Value.ShouldNotBeNull();
            result.Value.First().ShouldBe(9);
            result.Value.Second().ShouldBeOfType<CustomerViewModel>();
            ((CustomerViewModel)result.Value.Second()).Name.ShouldBe("Boycee");
            result.Value.Third().ShouldBeNull();
        }

        [Fact]
        public void ShouldMapFromAReadOnlyCollection()
        {
            var source = new PublicField<ReadOnlyCollection<string>>
            {
                Value = new ReadOnlyCollection<string>(new[] { "R", "A", "T", "M" })
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<object[]>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe("R", "A", "T", "M");
        }

        [Fact]
        public void ShouldMapFromAnEmptyReadOnlyCollection()
        {
            var source = new PublicField<ReadOnlyCollection<string>>
            {
                Value = new ReadOnlyCollection<string>(Enumerable<string>.EmptyArray)
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<ReadOnlyCollection<string>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.ShouldBeEmpty();
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
        public void ShouldCreateANewComplexTypeHashSet()
        {
            var source = new PublicField<MegaProduct[]>
            {
                Value = new[] { new MegaProduct { ProductId = "6387", HowMega = 100.00m } }
            };

            var result = Mapper.Map(source).ToANew<PublicProperty<HashSet<MegaProduct>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldHaveSingleItem();
            result.Value.First().ProductId.ShouldBe("6387");
            result.Value.First().HowMega.ShouldBe(100.00m);
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
#if NETCOREAPP2_0
                    .Map(ctx => ctx.Source.Value.Split(':', System.StringSplitOptions.None))
#else
                    .Map(ctx => ctx.Source.Value.Split(':'))
#endif
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
#if NETCOREAPP2_0
                    .Map(ctx => ctx.Source.Value.Split(':', System.StringSplitOptions.None))
#else
                    .Map(ctx => ctx.Source.Value.Split(':'))
#endif
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = null };
                var result = mapper.Map(source).ToANew<PublicField<int[]>>();

                result.Value.ShouldBeEmpty();
            }
        }

        [Fact]
        public void ShouldMapFromAnEnumerableToAWriteOnlyTarget()
        {
            var source = new PublicField<IEnumerable<int>> { Value = new[] { 1, 2, 3 } };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<List<string>>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("1", "2", "3");
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
        public void ShouldHandleANonNullReadOnlyNestedArray()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var readOnlyStrings = new[] { "1", "2" };

                mapper.CreateAReadOnlyFieldUsing(readOnlyStrings);

                var source = new PublicField<string[]> { Value = new[] { "3" } };
                var result = mapper.Map(source).ToANew<PublicReadOnlyField<string[]>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(readOnlyStrings);
                result.Value.ShouldBe("1", "2");
            }
        }

        [Fact]
        public void ShouldHandleANonNullReadOnlyNestedReadOnlyCollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var readOnlyInts = new ReadOnlyCollection<int>(new[] { 5, 5, 5 });

                mapper.CreateAReadOnlyPropertyUsing(readOnlyInts);

                var source = new PublicField<string[]> { Value = new[] { "3" } };
                var result = mapper.Map(source).ToANew<PublicReadOnlyProperty<ReadOnlyCollection<int>>>();

                result.Value.ShouldNotBeNull();
                result.Value.ShouldBeSameAs(readOnlyInts);
                result.Value.ShouldBe(5, 5, 5);
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
        public void ShouldHandleMultipleRuntimeTypedMembers()
        {
            var arraysSource = new PublicTwoFields<object, object>
            {
                Value1 = new[] { 1, 2, 3 },
                Value2 = new[] { "4", "5", "6" }
            };

            var arraysResult = Mapper.Map(arraysSource).ToANew<PublicTwoFields<long[], long[]>>();

            arraysResult.Value1.ShouldBe(1L, 2L, 3L);
            arraysResult.Value2.ShouldBe(4L, 5L, 6L);

            var listsSource = new PublicTwoFields<object, object>
            {
                Value1 = new List<int> { 7, 8, 9 },
                Value2 = new List<string> { "10", "11", "12" }
            };

            var listsResult = Mapper.Map(listsSource).ToANew<PublicTwoFields<long[], long[]>>();

            listsResult.Value1.ShouldBe(7L, 8L, 9L);
            listsResult.Value2.ShouldBe(10L, 11L, 12L);
        }

        [Fact]
        public void ShouldCreateAnEmptyCollectionByDefault()
        {
            var source = new PublicProperty<Collection<int>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<int>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeEmpty();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/115
        [Fact]
        public void ShouldMapNestedLists()
        {
            var source = new Issue115.A2
            {
                Id = 2,
                BB = new Issue115.B
                {
                    Id = 11,
                    CC = new List<Issue115.C>
                    {
                        new Issue115.C
                        {
                            Id = 111,
                            DD = new Issue115.D { Id = 111 }
                        }
                    }
                },
                CC = new Issue115.C
                {
                    Id = 111,
                    DD = new Issue115.D { Id = 112 }
                }
            };

            var result = Mapper.Map(source).ToANew<Issue115.A2Dto>();

            result.Id.ShouldBe(2);

            result.BB.ShouldNotBeNull();
            result.BB.Id.ShouldBe(11);
            result.BB.CC.ShouldHaveSingleItem();
            result.BB.CC[0].Id.ShouldBe(111);
            result.BB.CC[0].DD.ShouldNotBeNull().Id.ShouldBe(111);

            result.CC.ShouldNotBeNull();
            result.CC.Id.ShouldBe(111);
            result.CC.DD.ShouldNotBeNull().Id.ShouldBe(112);

        }

        private static class Issue115
        {
            // ReSharper disable ClassNeverInstantiated.Local
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // ReSharper disable CollectionNeverUpdated.Local
            public class A2
            {
                public int Id { get; set; }

                public B BB { get; set; }

                public C CC { get; set; }
            }

            public class B
            {
                public int Id { get; set; }

                public IList<C> CC { get; set; }
            }

            public class C
            {
                public int Id { get; set; }

                public D DD { get; set; }
            }

            public class D
            {
                public int Id { get; set; }
            }

            public class A2Dto
            {
                public int Id { get; set; }

                public BDto BB { get; set; }

                public CDto CC { get; set; }
            }

            public class BDto
            {
                public int Id { get; set; }

                public IList<CDto> CC { get; set; }
            }

            public class CDto
            {
                public int Id { get; set; }

                public DDto DD { get; set; }
            }

            public class DDto
            {
                public int Id { get; set; }
            }
            // ReSharper restore CollectionNeverUpdated.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore InconsistentNaming
            // ReSharper restore ClassNeverInstantiated.Local
        }
    }
}
