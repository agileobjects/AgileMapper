namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using Common;
    using Common.TestClasses;
    using TestClasses;
    using static TestClasses.Status;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToFlagsEnums
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASingleValueByteToAFlagsEnum()
        {
            var source = new PublicField<byte> { Value = (byte)InProgress };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(InProgress);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueShortToAFlagsEnum()
        {
            var source = new PublicField<short> { Value = (short)(InProgress | Assigned) };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldHaveFlag(InProgress);
            result.Value.ShouldHaveFlag(Assigned);
            result.Value.ShouldNotHaveFlag(Cancelled);
            result.Value.ShouldBe(InProgress | Assigned);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueNullableIntToAFlagsEnum()
        {
            var source = new PublicProperty<int?> { Value = (int)(New | Completed | Cancelled) };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | Completed | Cancelled);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableIntToANullableFlagsEnum()
        {
            var source = new PublicProperty<int?> { Value = default(int?) };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASingleValueLongToANullableFlagsEnum()
        {
            var source = new PublicProperty<long> { Value = (long)Removed };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBe(Removed);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueNumericCharacterToAFlagsEnum()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | Completed);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASingleValueNumericStringToAFlagsEnum()
        {
            var source = new PublicProperty<string> { Value = "4" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe((Status)4);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueMixedStringToAFlagsEnum()
        {
            var source = new PublicProperty<string> { Value = "9, InProgress, 4" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | InProgress | Completed);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueMixedObjectStringToAFlagsEnum()
        {
            var source = new PublicProperty<object> { Value = "New, InProgress, LaLaLa, 32" };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(New | InProgress | Removed);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASingleValueNullableFlagsEnumToAFlagsEnum()
        {
            var source = new PublicProperty<Status?> { Value = Cancelled };
            var result = Mapper.Map(source).ToANew<PublicField<Status>>();

            result.Value.ShouldBe(Cancelled);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMultiValueFlagsEnumToANullableFlagsEnum()
        {
            var source = new PublicProperty<Status> { Value = Assigned | Completed | Cancelled };
            var result = Mapper.Map(source).ToANew<PublicField<Status?>>();

            result.Value.ShouldBe(Assigned | Completed | Cancelled);
        }
    }
}
