namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using System.Globalization;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToInts
    {
        [Fact]
        public void ShouldMapASignedByteOverAnInt()
        {
            var source = new PublicProperty<sbyte> { Value = 16 };
            var target = Mapper.Map(source).Over(new PublicField<int> { Value = 6537 });

            target.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAByteToAnInt()
        {
            var source = new PublicProperty<byte> { Value = 32 };
            var target = Mapper.Map(source).ToANew<PublicField<int>>();

            target.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAShortToAnInt()
        {
            var source = new PublicProperty<short> { Value = 987 };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapALongToAnInt()
        {
            var source = new PublicField<long> { Value = 3215663 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToAnInt()
        {
            var source = new PublicField<uint> { Value = 32156 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToAnInt()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToANullableInt()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int?>>();

            result.Value.ShouldBe((int)source.Value);
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToAnInt()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToANullableInt()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<int?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberFloatOverAnInt()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToANullableInt()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToANew<PublicField<int?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatOverAnInt()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatOverAnInt()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var target = Mapper.Map(source).Over(new PublicProperty<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableInt()
        {
            var source = new PublicGetMethod<decimal>(53632.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<int?>>();

            result.Value.ShouldBe((int)source.GetValue());
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToAnInt()
        {
            var source = new PublicProperty<decimal> { Value = 637.5367m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverAnInt()
        {
            var source = new PublicProperty<decimal> { Value = decimal.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToAnInt()
        {
            var source = new PublicField<double> { Value = 63728 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableInt()
        {
            var source = new PublicField<double> { Value = double.MaxValue };
            var target = Mapper.Map(source).OnTo(new PublicField<int?>());

            target.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOverAnInt()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var target = Mapper.Map(source).Over(new PublicSetMethod<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnEnumOverAnInt()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var result = Mapper.Map(source).Over(new PublicProperty<int>());

            result.Value.ShouldBe((int)Title.Miss);
        }

        [Fact]
        public void ShouldMapACharacterToAnInt()
        {
            var source = new PublicProperty<char> { Value = '4' };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBe(4);
        }

        [Fact]
        public void ShouldMapAnUnparsableNullableCharacterToAnInt()
        {
            var source = new PublicProperty<char?> { Value = 'h' };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAParsableStringOnToAnInt()
        {
            const int VALUE = 63476387;
            var source = new PublicField<string> { Value = VALUE.ToString(CultureInfo.InvariantCulture) };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<int>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAParsableStringOverANullableInt()
        {
            const int VALUE = 9282625;
            var source = new PublicField<string> { Value = VALUE.ToString(CultureInfo.InvariantCulture) };
            var result = Mapper.Map(source).Over(new PublicSetMethod<int?>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableInt()
        {
            var source = new PublicProperty<string> { Value = "72389.9" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<int?>());

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToAnInt()
        {
            var source = new PublicProperty<string> { Value = "BAGPUSS" };
            var result = Mapper.Map(source).ToANew<PublicField<int>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverAnInt()
        {
            var source = new PublicField<string> { Value = "6738732.637" };
            var target = Mapper.Map(source).Over(new PublicField<int>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableInt()
        {
            var source = new PublicProperty<string> { Value = "BAGTASTIC" };
            var result = Mapper.Map(source).ToANew<PublicField<int?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnObjectIntOverAnInt()
        {
            var source = new PublicProperty<object> { Value = 123 };
            var target = new PublicField<int> { Value = 456 };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldMapAnObjectParseableStringOnToAnInt()
        {
            var source = new PublicProperty<object> { Value = "999" };
            var target = new PublicField<int> { Value = default(int) };
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe(999);
        }

        [Fact]
        public void ShouldMapANullObjectOverANullableInt()
        {
            var source = new PublicProperty<object> { Value = null };
            var target = new PublicField<int?> { Value = 555 };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAStringEnumerableToAnIntEnumerable()
        {
            IEnumerable<string> source = new[] { "1", "2", "3" };
            var result = Mapper.Map(source).ToANew<IEnumerable<int>>();

            result.ShouldBe(1, 2, 3);
        }
    }
}
