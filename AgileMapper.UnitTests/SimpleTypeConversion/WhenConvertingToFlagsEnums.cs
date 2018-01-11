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
        public void ShouldMapAMultiValueShortToAFlagsEnum()
        {
            var source = new PublicField<short> { Value = (short)(InProgress | Assigned) };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.HasFlag(InProgress).ShouldBeTrue();
            result.Value.HasFlag(Assigned).ShouldBeTrue();
            result.Value.HasFlag(Cancelled).ShouldBeFalse();
            result.Value.ShouldBe(InProgress | Assigned);
        }

        [Fact]
        public void ShouldMapAMultiValueNullableIntToAFlagsEnum()
        {
            var source = new PublicProperty<int?> { Value = (int)(New | Completed | Cancelled) };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | Completed | Cancelled);
        }

        [Fact]
        public void ShouldMapANullNullableIntToANullableFlagsEnum()
        {
            var source = new PublicProperty<int?> { Value = default(int?) };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapASingleValueLongToANullableFlagsEnum()
        {
            var source = new PublicProperty<long> { Value = (long)Removed };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBe(Removed);
        }

        [Fact]
        public void ShouldMapAMultiValueNumericCharacterToAFlagsEnum()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | Completed);
        }

        [Fact]
        public void ShouldMapASingleValueNumericStringToAFlagsEnum()
        {
            var source = new PublicProperty<string> { Value = "4" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe((Status)4);
        }

        [Fact]
        public void ShouldMapAMultiValueMixedStringToAFlagsEnum()
        {
            var source = new PublicProperty<string> { Value = "9, InProgress, 4" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | InProgress | Completed);
        }

        [Fact]
        public void ShouldMapAMultiValueMixedObjectStringToAFlagsEnum()
        {
            var source = new PublicProperty<object> { Value = "New, InProgress, LaLaLa, 32" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | InProgress | Removed);
        }

        [Fact]
        public void ShouldMapASingleValueNullableFlagsEnumToAFlagsEnum()
        {
            var source = new PublicProperty<Status?> { Value = Cancelled };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(Cancelled);
        }

        [Fact]
        public void ShouldMapAMultiValueFlagsEnumToANullableFlagsEnum()
        {
            var source = new PublicProperty<Status> { Value = Assigned | Completed | Cancelled };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBe(Assigned | Completed | Cancelled);
        }
    }
}
