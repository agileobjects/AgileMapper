namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamics
    {
        [Fact]
        public void ShouldMapToASimpleTypeMember()
        {
            dynamic source = new ExpandoObject();
            source.value = 123;

            var result = (PublicField<int>)Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBe(123);
        }
    }
}
