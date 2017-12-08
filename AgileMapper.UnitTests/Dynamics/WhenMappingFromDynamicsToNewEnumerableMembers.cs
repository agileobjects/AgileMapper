namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
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
    }
}
