namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using TestClasses;
    using Xunit;
    using static TestClasses.Status;

    public class WhenConvertingToFlagsEnums
    {
        [Fact]
        public void ShouldMapASingleValueByteToAFlagsEnum()
        {
            var source = new PublicField<byte> { Value = (byte)InProgress };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(InProgress);
        }

        [Fact]
        public void ShouldMapAMultiValueShortToAnEnum()
        {
            var source = new PublicField<short> { Value = (short)(InProgress | Assigned) };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.HasFlag(InProgress).ShouldBeTrue();
            result.Value.HasFlag(Assigned).ShouldBeTrue();
            result.Value.HasFlag(Cancelled).ShouldBeFalse();
            result.Value.ShouldBe(InProgress | Assigned);
        }
    }
}
