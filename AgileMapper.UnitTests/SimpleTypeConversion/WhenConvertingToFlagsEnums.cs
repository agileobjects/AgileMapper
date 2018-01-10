namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using TestClasses;
    using Xunit;

    public class WhenConvertingToFlagsEnums
    {
        [Fact]
        public void ShouldMapASingleValueByteToAFlagsEnum()
        {
            var source = new PublicField<byte> { Value = (byte)Status.InProgress };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(Status.InProgress);
        }
    }
}
