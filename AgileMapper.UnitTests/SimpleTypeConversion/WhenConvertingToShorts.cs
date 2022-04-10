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
    public class WhenConvertingToShorts
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASignedByteOntoAShort()
        {
            var source = new PublicProperty<sbyte> { Value = 8 };
            var target = Mapper.Map(source).OnTo(new PublicField<short>());

            target.Value.ShouldBe(8);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAByteToAShort()
        {
            var source = new PublicProperty<byte> { Value = 12 };
            var target = Mapper.Map(source).ToANew<PublicField<short>>();

            target.Value.ShouldBe(12);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntToAShort()
        {
            var source = new PublicProperty<int> { Value = 5267 };
            var result = Mapper.Map(source).ToANew<PublicField<short>>();

            result.Value.ShouldBe(5267);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedIntToAShort()
        {
            var source = new PublicField<uint> { Value = 627 };
            var result = Mapper.Map(source).ToANew<PublicProperty<short>>();

            result.Value.ShouldBe(627);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeLongToAShort()
        {
            var source = new PublicField<long> { Value = 12730 };
            var result = Mapper.Map(source).ToANew<PublicProperty<short>>();

            result.Value.ShouldBe(12730);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigLongToAShort()
        {
            var source = new PublicField<long> { Value = long.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeUnsignedLongToANullableShort()
        {
            var source = new PublicField<ulong> { Value = 7289 };
            var result = Mapper.Map(source).ToANew<PublicProperty<short?>>();

            result.Value.ShouldBe(7289);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeUnsignedLongToAShort()
        {
            var source = new PublicField<ulong> { Value = 32027 };
            var result = Mapper.Map(source).ToANew<PublicProperty<short>>();

            result.Value.ShouldBe(32027);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigUnsignedLongToANullableShort()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberFloatOverANullableShort()
        {
            var source = new PublicField<float> { Value = 532.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<short?>>();

            result.Value.ShouldBe(532);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToAShort()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToANew<PublicField<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberFloatOnToAShort()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var target = Mapper.Map(source).OnTo(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberFloatOverAShort()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var target = Mapper.Map(source).Over(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableShort()
        {
            var source = new PublicGetMethod<decimal>(5362.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<short?>>();

            result.Value.ShouldBe(5362);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToAShort()
        {
            var source = new PublicProperty<decimal> { Value = 637.5367m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverAShort()
        {
            var source = new PublicProperty<decimal> { Value = 637638.6373m };
            var target = Mapper.Map(source).Over(new PublicProperty<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberDoubleToAShort()
        {
            var source = new PublicField<double> { Value = 6728d };
            var result = Mapper.Map(source).ToANew<PublicProperty<short>>();

            result.Value.ShouldBe(6728);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberNullableDoubleOnToAShort()
        {
            var source = new PublicField<double?> { Value = double.MaxValue };
            var target = Mapper.Map(source).OnTo(new PublicField<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberDoubleOverANullableShort()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var target = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumOverAShort()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var result = Mapper.Map(source).Over(new PublicProperty<short>());

            result.Value.ShouldBe((short)source.Value);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterToAShort()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToANew<PublicField<short>>();

            result.Value.ShouldBe(9);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableNullableCharacterToANullableShort()
        {
            var source = new PublicProperty<char?> { Value = 'k' };
            var result = Mapper.Map(source).ToANew<PublicField<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableStringOnToAShort()
        {
            var source = new PublicField<string> { Value = "23387" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<short>());

            result.Value.ShouldBe(23387);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableStringOverANullableShort()
        {
            var source = new PublicField<string> { Value = "12625" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            result.Value.ShouldBe(12625);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableNonWholeNumberStringOverAShort()
        {
            var source = new PublicProperty<string> { Value = "2389.9" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<short?>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToAShort()
        {
            var source = new PublicProperty<string> { Value = "BLARGGHH" };
            var result = Mapper.Map(source).ToANew<PublicField<short>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableNumericStringOverAShort()
        {
            var source = new PublicField<string> { Value = "673873278282" };
            var target = Mapper.Map(source).Over(new PublicField<short>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToANullableShort()
        {
            var source = new PublicProperty<string> { Value = "HENDRIX" };
            var result = Mapper.Map(source).ToANew<PublicField<short?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringArrayToAShortEnumerable()
        {
            var source = new[] { "10", "20", "30" };
            var result = Mapper.Map(source).ToANew<IEnumerable<short>>();

            result.ShouldBe(s => (int)s, 10, 20, 30);
        }
    }
}
