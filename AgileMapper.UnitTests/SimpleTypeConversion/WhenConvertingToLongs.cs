namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using System.Globalization;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToLongs
    {
        [Fact]
        public void ShouldMapASignedByteOverALong()
        {
            var source = new PublicProperty<sbyte> { Value = 16 };
            var result = Mapper.Map(source).Over(new PublicField<long> { Value = 62687527 });

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAByteOntoALong()
        {
            var source = new PublicProperty<byte> { Value = 32 };
            var result = Mapper.Map(source).OnTo(new PublicField<long>());

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAShortToALong()
        {
            var source = new PublicProperty<short> { Value = 987 };
            var result = Mapper.Map(source).ToNew<PublicField<long>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnIntToALong()
        {
            var source = new PublicField<int> { Value = 32156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToALong()
        {
            var source = new PublicField<uint> { Value = 32156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToALong()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe((long)source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToANullableLong()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToNew<PublicProperty<long?>>();

            result.Value.ShouldBe((long)source.Value);
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToALong()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToANullableLong()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToNew<PublicProperty<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberFloatOverALong()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe((long)source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToANullableLong()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToNew<PublicField<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatOverALong()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatOverALong()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var result = Mapper.Map(source).Over(new PublicProperty<long>());

            result.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableLong()
        {
            var source = new PublicGetMethod<decimal>(53632.00m);
            var result = Mapper.Map(source).ToNew<PublicProperty<long?>>();

            result.Value.ShouldBe((long)source.GetValue());
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToALong()
        {
            var source = new PublicProperty<decimal> { Value = 637.5367m };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<long>());

            result.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverALong()
        {
            var source = new PublicProperty<decimal> { Value = decimal.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToALong()
        {
            var source = new PublicField<double> { Value = 63728 };
            var result = Mapper.Map(source).ToNew<PublicProperty<long>>();

            result.Value.ShouldBe((long)source.Value);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableLong()
        {
            var source = new PublicField<double> { Value = double.MaxValue };
            var result = Mapper.Map(source).OnTo(new PublicField<long?>());

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOverALong()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long>());

            result.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapAnEnumOverALong()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBe((long)source.Value);
        }

        [Fact]
        public void ShouldMapAParsableStringOnToALong()
        {
            const int VALUE = 63476387;
            var source = new PublicField<string> { Value = VALUE.ToString(CultureInfo.InvariantCulture) };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<long>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAParsableStringOverANullableLong()
        {
            const int VALUE = 9282625;
            var source = new PublicField<string> { Value = VALUE.ToString(CultureInfo.InvariantCulture) };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long?>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableLong()
        {
            var source = new PublicProperty<string> { Value = "6372389.63" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long?>());

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToALong()
        {
            var source = new PublicProperty<string> { Value = "CATFISH" };
            var result = Mapper.Map(source).ToNew<PublicField<long>>();

            result.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverALong()
        {
            var source = new PublicField<string> { Value = "6738732.637" };
            var target = Mapper.Map(source).Over(new PublicField<long>());

            target.Value.ShouldBe(default(long));
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableLong()
        {
            var source = new PublicProperty<string> { Value = "DOGFISH" };
            var result = Mapper.Map(source).ToNew<PublicField<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAStringArrayOnToALongCollection()
        {
            var source = new[] { "1", "1" };
            ICollection<long> target = new List<long> { 9, 9, 9 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(target);
            result.SequenceEqual(9L, 9L, 9L, 1L, 1L).ShouldBeTrue();
        }
    }
}
