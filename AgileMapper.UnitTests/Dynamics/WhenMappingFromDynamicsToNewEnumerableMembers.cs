namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewEnumerableMembers
    {
        [Fact]
        public void ShouldMapToASimpleTypeCollectionMember()
        {
            dynamic source = new ExpandoObject();
            source.Value = new[] { "a", "b", "c" };

            var result = (PublicField<char[]>)Mapper.Map(source).ToANew<PublicField<char[]>>();

            result.Value.ShouldBe('a', 'b', 'c');
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableMember()
        {
            dynamic source = new ExpandoObject();

            source.Value = new[]
            {
                new PublicField<string> { Value = "1" },
                new PublicField<string> { Value = "2" },
                new PublicField<string> { Value = "3" }
            };

            var result = (PublicProperty<IEnumerable<PublicField<int>>>)Mapper
                .Map(source)
                .ToANew<PublicProperty<IEnumerable<PublicField<int>>>>();

            result.Value.ShouldBe(pf => pf.Value, 1, 2, 3);
        }
    }
}
