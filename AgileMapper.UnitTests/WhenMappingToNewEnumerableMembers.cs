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
        public void ShouldApplyAConfiguredExpressionToAnEnumerable()
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
        public void ShouldCreateAnEmptyCollectionByDefault()
        {
            var source = new PublicProperty<Collection<int>> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<int>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeEmpty();
        }
    }
}
