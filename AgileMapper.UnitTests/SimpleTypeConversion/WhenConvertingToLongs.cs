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
    public class WhenConvertingToLongs
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASignedByteOverALong()
        {
            var source = new PublicProperty<sbyte> { Value = 16 };
            var result = Mapper.Map(source).Over(new PublicField<long> { Value = 62687527 });

            result.Value.ShouldBe(16);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAByteOntoALong()
        {
            var source = new PublicProperty<byte> { Value = 32 };
            var result = Mapper.Map(source).OnTo(new PublicField<long>());

            result.Value.ShouldBe(32);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAShortToALong()
        {
            var source = new PublicProperty<short> { Value = 987 };
            var result = Mapper.Map(source).ToANew<PublicField<long>>();

            result.Value.ShouldBe(987);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntToALong()
        {
            var source = new PublicField<int> { Value = 32156 };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBe(32156);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnsignedIntToALong()
        {
            var source = new PublicField<uint> { Value = 32658 };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBe(32658);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeUnsignedLongToALong()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBe(9292726);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeUnsignedLongToANullableLong()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToANew<PublicProperty<long?>>();

            result.Value.ShouldBe(9383625);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigUnsignedLongToALong()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigUnsignedLongToANullableLong()
        {
            var source = new PublicField<ulong> { Value = ulong.MaxValue };
            var result = Mapper.Map(source).ToANew<PublicProperty<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberFloatOverALong()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBe(8532);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToANullableLong()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToANew<PublicField<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberFloatOverALong()
        {
            var source = new PublicProperty<float> { Value = float.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberFloatOverALong()
        {
            var source = new PublicProperty<float> { Value = float.MinValue };
            var result = Mapper.Map(source).Over(new PublicProperty<long>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableLong()
        {
            var source = new PublicGetMethod<decimal>(53632.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<long?>>();

            result.Value.ShouldBe(53632);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToALong()
        {
            var source = new PublicProperty<decimal> { Value = 637.5367m };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<long>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverALong()
        {
            var source = new PublicProperty<decimal> { Value = decimal.MaxValue };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInRangeWholeNumberDoubleToALong()
        {
            var source = new PublicField<double> { Value = 63728 };
            var result = Mapper.Map(source).ToANew<PublicProperty<long>>();

            result.Value.ShouldBe(63728);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableLong()
        {
            var source = new PublicField<double> { Value = double.MaxValue };
            var result = Mapper.Map(source).OnTo(new PublicField<long?>());

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapATooSmallWholeNumberDoubleOverALong()
        {
            var source = new PublicProperty<double> { Value = double.MinValue };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumOverALong()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var target = Mapper.Map(source).Over(new PublicProperty<long>());

            target.Value.ShouldBe((long)Title.Miss);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterToANullableLong()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToANew<PublicField<long?>>();

            result.Value.ShouldBe(9);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableCharacterToALong()
        {
            var source = new PublicProperty<char> { Value = 't' };
            var result = Mapper.Map(source).ToANew<PublicField<long>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableStringOnToALong()
        {
            var source = new PublicField<string> { Value = "63476387" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<long>());

            result.Value.ShouldBe(63476387);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableStringOverANullableLong()
        {
            var source = new PublicField<string> { Value = "9282625" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long?>());

            result.Value.ShouldBe(9282625);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableLong()
        {
            var source = new PublicProperty<string> { Value = "6372389.63" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<long?>());

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToALong()
        {
            var source = new PublicProperty<string> { Value = "CATFISH" };
            var result = Mapper.Map(source).ToANew<PublicField<long>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableNumericStringOverALong()
        {
            var source = new PublicField<string> { Value = "6738732.637" };
            var target = Mapper.Map(source).Over(new PublicField<long>());

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnUnparsableStringToANullableLong()
        {
            var source = new PublicProperty<string> { Value = "DOGFISH" };
            var result = Mapper.Map(source).ToANew<PublicField<long?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringArrayOnToALongCollection()
        {
            var source = new[] { "1", "1" };
            ICollection<long> target = new List<long> { 9, 9, 9 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(target);
            result.ShouldBe(9L, 9L, 9L, 1L, 1L);
        }
    }
}
