namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToBytes
    {
        [Fact]
        public void ShouldMapASignedByteOnToAByte()
        {
            var source = new PublicProperty<sbyte> { Value = 6 };
            var target = Mapper.Map(source).OnTo(new PublicField<byte>());

            target.Value.ShouldBe(6);
        }

        [Fact]
        public void ShouldMapANullableShortToAByte()
        {
            var source = new PublicProperty<short?> { Value = 32 };
            var target = Mapper.Map(source).ToANew<PublicField<byte>>();

            target.Value.ShouldBe(32);
        }

        [Fact]
        public void ShouldMapAnUnsignedShortToANullableByte()
        {
            var source = new PublicProperty<ushort> { Value = 60 };
            var target = Mapper.Map(source).ToANew<PublicField<byte?>>();

            target.Value.ShouldBe(60);
        }

        [Fact]
        public void ShouldMapAnInRangeIntToAByte()
        {
            var source = new PublicProperty<int> { Value = 23 };
            var result = Mapper.Map(source).ToANew<PublicField<byte>>();

            result.Value.ShouldBe(23);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedIntToAByte()
        {
            var source = new PublicField<uint> { Value = 250 };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte>>();

            result.Value.ShouldBe(250);
        }

        [Fact]
        public void ShouldMapATooBigIntToAByte()
        {
            var source = new PublicField<int> { Value = 276 };
            var result = Mapper.Map(source).ToANew<PublicField<byte>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeLongToAByte()
        {
            var source = new PublicField<long> { Value = 166 };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte>>();

            result.Value.ShouldBe(166);
        }

        [Fact]
        public void ShouldMapATooBigLongToAByte()
        {
            var source = new PublicField<long> { Value = 637 };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToANullableByte()
        {
            var source = new PublicField<ulong> { Value = 100 };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte?>>();

            result.Value.ShouldBe(100);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongOverAByte()
        {
            var source = new PublicField<ulong> { Value = 36 };
            var target = new PublicProperty<byte> { Value = 72 };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe(36);
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToANullableByte()
        {
            var source = new PublicField<ulong> { Value = 6328 };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberFloatOverANullableByte()
        {
            var source = new PublicField<float> { Value = 52.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte?>>();

            result.Value.ShouldBe(52);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToAByte()
        {
            var source = new PublicProperty<float?> { Value = 37.21f };
            var result = Mapper.Map(source).ToANew<PublicField<byte>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatOnToAByte()
        {
            var source = new PublicProperty<float> { Value = 722f };
            var target = Mapper.Map(source).OnTo(new PublicProperty<byte>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatOverAByte()
        {
            var source = new PublicProperty<float> { Value = -354f };
            var target = Mapper.Map(source).Over(new PublicProperty<byte>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableByte()
        {
            var source = new PublicGetMethod<decimal>(62.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<short?>>();

            result.Value.ShouldBe(62);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToAByte()
        {
            var source = new PublicProperty<decimal> { Value = 67.5367m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<byte>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverAByte()
        {
            var source = new PublicProperty<decimal> { Value = 755.009m };
            var target = Mapper.Map(source).Over(new PublicProperty<byte>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToAByte()
        {
            var source = new PublicField<double> { Value = 28d };
            var result = Mapper.Map(source).ToANew<PublicProperty<byte>>();

            result.Value.ShouldBe(28);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberNullableDoubleOnToAByte()
        {
            var source = new PublicField<double?> { Value = 89292d };
            var target = Mapper.Map(source).OnTo(new PublicField<byte>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOnToANullableByte()
        {
            var source = new PublicProperty<double> { Value = -83872d };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<byte?>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapABoolFalseToByteZero()
        {
            var source = new PublicProperty<bool> { Value = false };
            var result = Mapper.Map(source).Over(new PublicField<byte> { Value = 1 });

            result.Value.ShouldBe(0);
        }

        [Fact]
        public void ShouldMapAnEnumOverAByte()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var result = Mapper.Map(source).Over(new PublicProperty<byte>());

            result.Value.ShouldBe((byte)source.Value);
        }

        [Fact]
        public void ShouldMapACharacterToAByte()
        {
            var source = new PublicProperty<char> { Value = '2' };
            var result = Mapper.Map(source).ToANew<PublicField<byte>>();

            result.Value.ShouldBe(2);
        }

        [Fact]
        public void ShouldMapAnUnparsableNullableCharacterToANullableByte()
        {
            var source = new PublicProperty<char?> { Value = 'z' };
            var result = Mapper.Map(source).ToANew<PublicField<byte?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapACharacterEnumerableMemberToANullableByteCollection()
        {
            var source = new PublicProperty<IEnumerable<char>> { Value = new[] { '3', '2', '1' } };
            var result = Mapper.Map(source).ToANew<PublicField<ICollection<byte?>>>();

            // ReSharper disable once PossibleInvalidOperationException
            result.Value.ShouldBe(s => (int)s, 3, 2, 1);
        }

        [Fact]
        public void ShouldMapAParsableStringOverAByte()
        {
            var source = new PublicField<string> { Value = "97" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<byte>());

            result.Value.ShouldBe(97);
        }

        [Fact]
        public void ShouldMapAParsableStringOverANullableByte()
        {
            var source = new PublicField<string> { Value = "225" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<byte?>());

            result.Value.ShouldBe(225);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverAByte()
        {
            var source = new PublicProperty<string> { Value = "89.9" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<byte>());

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToAByte()
        {
            var source = new PublicProperty<string> { Value = "LALALA" };
            var result = Mapper.Map(source).ToANew<PublicField<byte>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverANullableByte()
        {
            var source = new PublicField<string> { Value = "8329" };
            var target = Mapper.Map(source).Over(new PublicField<byte?>());

            target.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableByte()
        {
            var source = new PublicProperty<string> { Value = "Lennon" };
            var result = Mapper.Map(source).ToANew<PublicField<byte?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAStringArrayToAByteEnumerable()
        {
            var source = new[] { "9", "8", "7" };
            var result = Mapper.Map(source).ToANew<IEnumerable<byte>>();

            result.ShouldBe(s => s, 9, 8, 7);
        }
    }
}
