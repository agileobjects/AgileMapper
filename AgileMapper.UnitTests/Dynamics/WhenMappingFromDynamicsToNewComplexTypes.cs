namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewComplexTypes
    {
        [Fact]
        public void ShouldMapToASimpleTypeMember()
        {
            dynamic source = new ExpandoObject();
            source.value = 123;

            var result = (PublicField<int>)Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldConvertASimpleTypeMemberValue()
        {
            dynamic source = new ExpandoObject();
            source.Value = "728";

            var result = (PublicField<long>)Mapper.Map(source).ToANew<PublicField<long>>();

            result.Value.ShouldBe(728L);
        }
    }
}
