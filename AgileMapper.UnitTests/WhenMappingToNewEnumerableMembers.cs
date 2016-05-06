namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
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
            var result = Mapper.Map(source).ToNew<PublicField<List<string>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.SequenceEqual(source.Value).ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreateANewIntArray()
        {
            var source = new PublicField<int[]> { Value = new[] { 9, 8, 7, 6, 5 } };
            var result = Mapper.Map(source).ToNew<PublicField<int[]>>();

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

            var result = Mapper.Map(source).ToNew<PublicField<IEnumerable<PersonViewModel>>>();

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe(source.Value.Select(p => p.Name), pvm => pvm.Name);
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionToAnEnumerable()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int[]>>()
                    .Map(x => x.Value.Split(':'))
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = "8:7:6:5" };
                var result = mapper.Map(source).ToNew<PublicField<int[]>>();

                result.Value.ShouldBe(8, 7, 6, 5);
            }
        }

        [Fact]
        public void ShouldHandleANullSourceMemberInAConfiguredEnumerableSource()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.When.Mapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int[]>>()
                    .Map(x => x.Value.Split(','))
                    .To(x => x.Value);

                var source = new PublicProperty<string> { Value = null };
                var result = mapper.Map(source).ToNew<PublicField<int[]>>();

                result.Value.ShouldBeDefault();
            }
        }
    }
}
