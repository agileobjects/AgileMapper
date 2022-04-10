namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToDecimals
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASignedByteOverADecimal()
        {
            var source = new PublicProperty<sbyte> { Value = 83 };
            var result = Mapper.Map(source).Over(new PublicField<decimal> { Value = 64738 });

            result.Value.ShouldBe(83);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAByteOntoADecimal()
        {
            var source = new PublicProperty<byte> { Value = 99 };
            var result = Mapper.Map(source).OnTo(new PublicField<decimal>());

            result.Value.ShouldBe(99);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAShortToADecimal()
        {
            var source = new PublicProperty<short> { Value = 9287 };
            var result = Mapper.Map(source).ToANew<PublicField<decimal>>();

            result.Value.ShouldBe(9287);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntToADecimal()
        {
            var source = new PublicField<int> { Value = 32156 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(32156);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedIntToADecimal()
        {
            var source = new PublicField<uint> { Value = 32658 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(32658);
        }
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapALongToADecimal()
        {
            var source = new PublicField<long> { Value = 3156 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(3156);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedLongToADecimal()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(9292726);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedLongToANullableDecimal()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal?>>();

            result.Value.ShouldBe(9383625);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAWholeNumberFloatOverADecimal()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(8532);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonWholeNumberNullableFloatToANullableDecimal()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBe(73.62);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberFloatToANullableDecimal()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberFloatToANullableDecimal()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAWholeNumberDecimalToANullableDecimal()
        {
            var source = new PublicGetMethod<decimal>(5332.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal?>>();

            result.Value.ShouldBe(5332);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonWholeNumberNullableDecimalOverADecimal()
        {
            var source = new PublicProperty<decimal> { Value = 938378.637m };
            var target = Mapper.Map(source).Over(new PublicProperty<decimal>());

            target.Value.ShouldBe(938378.637);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberDoubleToADecimal()
        {
            var source = new PublicField<double> { Value = 637128 };
            var result = Mapper.Map(source).ToANew<PublicProperty<decimal>>();

            result.Value.ShouldBe(637128);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableDecimal()
        {
            var source = new PublicField<double> { Value = double.MaxValue };
            var result = Mapper.Map(source).OnTo(new PublicField<decimal?>());

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberDoubleOverADecimal()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var result = Mapper.Map(source).Over(new PublicSetMethod<decimal>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableBoolFalseToNullableDecimalOne()
        {
            var source = new PublicProperty<bool?> { Value = false };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBe(decimal.Zero);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumOverADecimal()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var target = Mapper.Map(source).Over(new PublicProperty<decimal>());

            target.Value.ShouldBe((decimal)Title.Miss);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterToANullableDecimal()
        {
            var source = new PublicProperty<char> { Value = '3' };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBe(3);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableCharacterToADecimal()
        {
            var source = new PublicProperty<char> { Value = 'g' };
            var result = Mapper.Map(source).ToANew<PublicField<decimal>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableWholeNumberStringOnToADecimal()
        {
            var source = new PublicField<string> { Value = "6347687" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<decimal>());

            result.Value.ShouldBe(6347687);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableDecimal()
        {
            var source = new PublicProperty<string> { Value = "6372389.63" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<decimal?>());

            result.Value.ShouldBe(6372389.63);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToADecimal()
        {
            var source = new PublicProperty<string> { Value = "GIBLETS" };
            var result = Mapper.Map(source).ToANew<PublicField<decimal>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableNumericStringOverADecimal()
        {
            var source = new PublicField<string> { Value = "938383737383839209220202928287272727282829292092020202020296738732.637" };
            var target = Mapper.Map(source).Over(new PublicField<decimal>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToANullableDecimal()
        {
            var source = new PublicProperty<string> { Value = "PFFFFFT" };
            var result = Mapper.Map(source).ToANew<PublicField<decimal?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
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
