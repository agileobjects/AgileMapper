namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToBools
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASignedByteOneOnToABool()
        {
            var source = new PublicProperty<sbyte> { Value = 1 };
            var target = Mapper.Map(source).OnTo(new PublicField<bool>());

            target.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableShortZeroToABool()
        {
            var source = new PublicProperty<short?> { Value = 0 };
            var target = Mapper.Map(source).ToANew<PublicField<bool>>();

            target.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedShortZeroToANullableBool()
        {
            var source = new PublicProperty<ushort> { Value = 0 };
            var target = Mapper.Map(source).ToANew<PublicField<bool?>>();

            target.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntOneToABool()
        {
            var source = new PublicProperty<int> { Value = 1 };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBe(23);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedIntToABool()
        {
            var source = new PublicField<uint> { Value = 250 };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnNonBoolIntToABool()
        {
            var source = new PublicField<int> { Value = -1 };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapALongZeroToABool()
        {
            var source = new PublicField<long> { Value = 0 };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool>>();

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolLongToANullableBool()
        {
            var source = new PublicField<long> { Value = 637 };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedLongZeroToANullableBool()
        {
            var source = new PublicField<ulong> { Value = 0 };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedLongOneOverABool()
        {
            var source = new PublicField<ulong> { Value = 1 };
            var target = new PublicProperty<bool> { Value = false };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolUnsignedLongToANullableBool()
        {
            var source = new PublicField<ulong> { Value = 63 };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnNonBoolWholeNumberFloatOverANullableBool()
        {
            var source = new PublicField<float> { Value = 52.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnNonWholeNumberNullableFloatToABool()
        {
            var source = new PublicProperty<float?> { Value = 1.01f };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAFloatZeroOnToABool()
        {
            var source = new PublicProperty<float> { Value = 0f };
            var target = Mapper.Map(source).OnTo(new PublicProperty<bool>());

            target.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolWholeNumberFloatOverABool()
        {
            var source = new PublicProperty<float> { Value = -34f };
            var target = Mapper.Map(source).Over(new PublicProperty<bool>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnDecimalOneToANullableBool()
        {
            var source = new PublicGetMethod<decimal>(1);
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnNonBoolNonWholeNumberDecimalOnToABool()
        {
            var source = new PublicProperty<decimal> { Value = 62.7m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<bool>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableDecimalZeroOverABool()
        {
            var source = new PublicProperty<decimal> { Value = decimal.Zero };
            var target = Mapper.Map(source).Over(new PublicProperty<bool> { Value = true });

            target.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapADoubleOneToABool()
        {
            var source = new PublicField<double> { Value = 1d };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool>>();

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolWholeNumberNullableDoubleOnToABool()
        {
            var source = new PublicField<double?> { Value = 892d };
            var target = Mapper.Map(source).OnTo(new PublicField<bool>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapADoubleZeroOnToANullableBool()
        {
            var source = new PublicProperty<double> { Value = 0.0d };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<bool?>());

            target.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterOneToABool()
        {
            var source = new PublicProperty<char> { Value = '1' };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterZeroToANullableBool()
        {
            var source = new PublicProperty<char> { Value = '0' };
            var result = Mapper.Map(source).ToANew<PublicField<bool?>>();

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableNullableCharacterToANullableBool()
        {
            var source = new PublicProperty<char?> { Value = 'x' };
            var result = Mapper.Map(source).ToANew<PublicField<bool?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterEnumerableMemberToANullableByteCollection()
        {
            var source = new PublicProperty<IEnumerable<char>> { Value = new[] { '1', 'f', '0' } };
            var result = Mapper.Map(source).ToANew<PublicField<ICollection<bool?>>>();

            result.Value.ShouldBe(true, null, false);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringTrueOnToABool()
        {
            var source = new PublicProperty<string> { Value = "true" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<bool>());

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringTrueOnToANullableBoolCaseInsensitively()
        {
            var source = new PublicProperty<string> { Value = "tRuE" };
            var result = Mapper.Map(source).OnTo(new PublicField<bool?>());

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringFalseOnToANullableBool()
        {
            var source = new PublicProperty<string> { Value = "false" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<bool?>());

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringOneOverABool()
        {
            var source = new PublicField<string> { Value = "1" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<bool>());

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringZeroOverANullableBool()
        {
            var source = new PublicField<string> { Value = "0" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<bool?>());

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolStringToABool()
        {
            var source = new PublicProperty<string> { Value = "LALA" };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonBoolStringOverANullableBool()
        {
            var source = new PublicField<string> { Value = "629" };
            var target = Mapper.Map(source).Over(new PublicField<bool?>());

            target.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringArrayToABoolCollection()
        {
            var source = new[] { "9", "1", "0" };
            var result = Mapper.Map(source).ToANew<Collection<bool>>();

            result.ShouldBe(false, true, false);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableBoolTrueToABool()
        {
            var source = new PublicProperty<bool?> { Value = true };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableBoolNullToABool()
        {
            var source = new PublicProperty<bool?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicField<bool>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectStringTrueToABool()
        {
            var source = new PublicProperty<object> { Value = "true" };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool>>();

            result.Value.ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectStringZeroToANullableBool()
        {
            var source = new PublicProperty<object> { Value = "0" };
            var result = Mapper.Map(source).ToANew<PublicField<bool?>>();

            result.Value.ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectNonBoolStringToANullableBool()
        {
            var source = new PublicProperty<object> { Value = "jfkdlk" };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectLongOneToANullableBool()
        {
            var source = new PublicProperty<object> { Value = 1L };
            var result = Mapper.Map(source).ToANew<PublicProperty<bool?>>();

            result.Value.ShouldBeTrue();
        }
    }
}
