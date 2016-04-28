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
        public void ShouldCreateANewComplexTypeEnumerable()
        {
            var source = new PublicField<Person[]>
            {
                Value = new[] { new Person { Name = "Jack" }, new Person { Name = "Sparrow" } }
            };

            var result = Mapper.Map(source).ToNew<PublicField<IEnumerable<PersonViewModel>>>();

            result.Value.ShouldNotBeNull();

            result.Value
                .Select(pvm => pvm.Name)
                .SequenceEqual(source.Value.Select(p => p.Name))
                .ShouldBeTrue();
        }
    }
}
