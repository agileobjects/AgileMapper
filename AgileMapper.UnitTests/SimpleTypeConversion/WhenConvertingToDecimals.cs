namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToDecimals
    {
        [Fact]
        public void ShouldMapASignedByteOverADecimal()
        {
            var source = new PublicProperty<sbyte> { Value = 83 };
            var result = Mapper.Map(source).Over(new PublicField<decimal> { Value = 64738 });

            result.Value.ShouldBe(83);
        }

        [Fact]
        public void ShouldMapAByteOntoADecimal()
        {
            var source = new PublicProperty<byte> { Value = 99 };
            var result = Mapper.Map(source).OnTo(new PublicField<decimal>());

            result.Value.ShouldBe(99);
        }

        [Fact]
        public void ShouldMapAShortToADecimal()
        {
            var source = new PublicProperty<short> { Value = 9287 };
            var result = Mapper.Map(source).ToNew<PublicField<decimal>>();

            result.Value.ShouldBe(9287);
        }

        [Fact]
        public void ShouldMapAnIntToADecimal()
        {
            var source = new PublicField<int> { Value = 32156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(32156);
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToADecimal()
        {
            var source = new PublicField<uint> { Value = 32658 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(32658);
        }
        [Fact]
        public void ShouldMapALongToADecimal()
        {
            var source = new PublicField<long> { Value = 3156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(3156);
        }

        [Fact]
        public void ShouldMapAnUnsignedLongToADecimal()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(9292726);
        }

        [Fact]
        public void ShouldMapAnUnsignedLongToANullableDecimal()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal?>>();

            result.Value.ShouldBe(9383625);
        }

        [Fact]
        public void ShouldMapAWholeNumberFloatOverADecimal()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(8532);
        }

        [Fact]
        public void ShouldMapANonWholeNumberNullableFloatToANullableDecimal()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToNew<PublicField<decimal?>>();

            result.Value.ShouldBe(73.62);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatToANullableDecimal()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var result = Mapper.Map(source).ToNew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatToANullableDecimal()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var result = Mapper.Map(source).ToNew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAWholeNumberDecimalToANullableDecimal()
        {
            var source = new PublicGetMethod<decimal>(5332.00m);
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal?>>();

            result.Value.ShouldBe(5332);
        }

        [Fact]
        public void ShouldMapANonWholeNumberNullableDecimalOverADecimal()
        {
            var source = new PublicProperty<decimal> { Value = 938378.637m };
            var target = Mapper.Map(source).Over(new PublicProperty<decimal>());

            target.Value.ShouldBe(938378.637);
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToADecimal()
        {
            var source = new PublicField<double> { Value = 637128 };
            var result = Mapper.Map(source).ToNew<PublicProperty<decimal>>();

            result.Value.ShouldBe(637128);
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableDecimal()
        {
            var source = new PublicField<double> { Value = double.MaxValue };
            var result = Mapper.Map(source).OnTo(new PublicField<decimal?>());

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOverADecimal()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var result = Mapper.Map(source).Over(new PublicSetMethod<decimal>());

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnEnumOverADecimal()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var target = Mapper.Map(source).Over(new PublicProperty<decimal>());

            target.Value.ShouldBe((decimal)Title.Miss);
        }

        [Fact]
        public void ShouldMapACharacterToANullableDecimal()
        {
            var source = new PublicProperty<char> { Value = '3' };
            var result = Mapper.Map(source).ToNew<PublicField<decimal?>>();

            result.Value.ShouldBe(3);
        }

        [Fact]
        public void ShouldMapAnUnparsableCharacterToADecimal()
        {
            var source = new PublicProperty<char> { Value = 'g' };
            var result = Mapper.Map(source).ToNew<PublicField<decimal>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAParsableWholeNumberStringOnToADecimal()
        {
            var source = new PublicField<string> { Value = "6347687" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<decimal>());

            result.Value.ShouldBe(6347687);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableDecimal()
        {
            var source = new PublicProperty<string> { Value = "6372389.63" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<decimal?>());

            result.Value.ShouldBe(6372389.63);
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToADecimal()
        {
            var source = new PublicProperty<string> { Value = "GIBLETS" };
            var result = Mapper.Map(source).ToNew<PublicField<decimal>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverADecimal()
        {
            var source = new PublicField<string> { Value = "938383737383839209220202928287272727282829292092020202020296738732.637" };
            var target = Mapper.Map(source).Over(new PublicField<decimal>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableDecimal()
        {
            var source = new PublicProperty<string> { Value = "PFFFFFT" };
            var result = Mapper.Map(source).ToNew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapACharacterArrayOnToADecimalList()
        {
            var source = new[] { '1', '9', '7' };
            IList<decimal> target = new List<decimal> { 9, 9 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(target);
            result.ShouldBe(9m, 9m, 1m, 7m);
        }
    }
}
