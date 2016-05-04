namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToShorts
    {
        [Fact]
        public void ShouldMapASignedByteOntoAShort()
        {
            var source = new PublicProperty<sbyte> { Value = 8 };
            var target = Mapper.Map(source).OnTo(new PublicField<short>());

            target.Value.ShouldBe(8);
        }

        [Fact]
        public void ShouldMapAByteToAShort()
        {
            var source = new PublicProperty<byte> { Value = 12 };
            var target = Mapper.Map(source).ToNew<PublicField<short>>();

            target.Value.ShouldBe(12);
        }

        [Fact]
        public void ShouldMapAnIntToAShort()
        {
            var source = new PublicProperty<int> { Value = 5267 };
            var result = Mapper.Map(source).ToNew<PublicField<short>>();

            result.Value.ShouldBe(5267);
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToAShort()
        {
            var source = new PublicField<uint> { Value = 627 };
            var result = Mapper.Map(source).ToNew<PublicProperty<short>>();

            result.Value.ShouldBe(627);
        }

        [Fact]
        public void ShouldMapAnInRangeLongToAShort()
        {
            var source = new PublicField<long> { Value = 12730 };
            var result = Mapper.Map(source).ToNew<PublicProperty<short>>();

            result.Value.ShouldBe(12730);
        }

        [Fact]
        public void ShouldMapATooBigLongToAShort()
        {
            var source = new PublicField<long> { Value = long.MaxValue };
            var result = Mapper.Map(source).ToNew<PublicProperty<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToANullableShort()
        {
            var source = new PublicField<ulong> { Value = 7289 };
            var result = Mapper.Map(source).ToNew<PublicProperty<short?>>();

            result.Value.ShouldBe(7289);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToAShort()
        {
            var source = new PublicField<ulong> { Value = 32027 };
            var result = Mapper.Map(source).ToNew<PublicProperty<short>>();

            result.Value.ShouldBe(32027);
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToANullableShort()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToNew<PublicProperty<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberFloatOverANullableShort()
        {
            var source = new PublicField<float> { Value = 532.00f };
            var result = Mapper.Map(source).ToNew<PublicProperty<short?>>();

            result.Value.ShouldBe(532);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToAShort()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToNew<PublicField<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatOnToAShort()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var target = Mapper.Map(source).OnTo(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatOverAShort()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var target = Mapper.Map(source).Over(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableShort()
        {
            var source = new PublicGetMethod<decimal>(5362.00m);
            var result = Mapper.Map(source).ToNew<PublicProperty<short?>>();

            result.Value.ShouldBe(5362);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToAShort()
        {
            var source = new PublicProperty<decimal> { Value = 637.5367m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverAShort()
        {
            var source = new PublicProperty<decimal> { Value = decimal.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToAShort()
        {
            var source = new PublicField<double> { Value = 6728d };
            var result = Mapper.Map(source).ToNew<PublicProperty<short>>();

            result.Value.ShouldBe(6728);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberNullableDoubleOnToAShort()
        {
            var source = new PublicField<double?> { Value = double.MaxValue };
            var target = Mapper.Map(source).OnTo(new PublicField<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOverANullableShort()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var target = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnEnumOverAShort()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var result = Mapper.Map(source).Over(new PublicProperty<short>());

            result.Value.ShouldBe((short)source.Value);
        }

        [Fact]
        public void ShouldMapACharacterToAShort()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToNew<PublicField<short>>();

            result.Value.ShouldBe(9);
        }

        [Fact]
        public void ShouldMapAnUnparsableNullableCharacterToANullableShort()
        {
            var source = new PublicProperty<char?> { Value = 'k' };
            var result = Mapper.Map(source).ToNew<PublicField<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAParsableStringOnToAShort()
        {
            var source = new PublicField<string> { Value = "23387" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<short>());

            result.Value.ShouldBe(23387);
        }

        [Fact]
        public void ShouldMapAParsableStringOverANullableShort()
        {
            var source = new PublicField<string> { Value = "12625" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            result.Value.ShouldBe(12625);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverAShort()
        {
            var source = new PublicProperty<string> { Value = "2389.9" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToAShort()
        {
            var source = new PublicProperty<string> { Value = "BLARGGHH" };
            var result = Mapper.Map(source).ToNew<PublicField<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverAShort()
        {
            var source = new PublicField<string> { Value = "673873278282" };
            var target = Mapper.Map(source).Over(new PublicField<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableShort()
        {
            var source = new PublicProperty<string> { Value = "HENDRIX" };
            var result = Mapper.Map(source).ToNew<PublicField<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAStringArrayToAShortEnumerable()
        {
            var source = new[] { "10", "20", "30" };
            var result = Mapper.Map(source).ToNew<IEnumerable<short>>();

            result.ShouldBe(s => (int)s, 10, 20, 30);
        }
    }
}
