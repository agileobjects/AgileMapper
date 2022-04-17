namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenConvertingToCharacters
    {
        [Fact]
        public void ShouldMapASignedByteOverACharacter()
        {
            var source = new PublicProperty<sbyte> { Value = 6 };
            var target = Mapper.Map(source).Over(new PublicField<char> { Value = '5' });

            target.Value.ShouldBe('6');
        }

        [Fact]
        public void ShouldMapAByteToACharacter()
        {
            var source = new PublicProperty<byte> { Value = 3 };
            var target = Mapper.Map(source).ToANew<PublicField<char>>();

            target.Value.ShouldBe('3');
        }

        [Fact]
        public void ShouldMapAShortToACharacter()
        {
            var source = new PublicProperty<short> { Value = 8 };
            var result = Mapper.Map(source).ToANew<PublicField<char>>();

            result.Value.ShouldBe('8');
        }

        [Fact]
        public void ShouldMapALongToACharacter()
        {
            var source = new PublicField<long> { Value = 7L };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe('7');
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToACharacter()
        {
            var source = new PublicField<uint> { Value = 6 };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe('6');
        }

        [Fact]
        public void ShouldMapAnIntToACharacter()
        {
            var source = new PublicProperty<int> { Value = 4 };
            var result = Mapper.Map(source).ToANew<PublicField<char>>();

            result.Value.ShouldBe('4');
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToACharacter()
        {
            var source = new PublicField<ulong> { Value = 9 };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe('9');
        }

        [Fact]
        public void ShouldMapAnInRangeUnsignedLongToANullableCharacter()
        {
            var source = new PublicField<ulong> { Value = 1 };
            var result = Mapper.Map(source).ToANew<PublicProperty<char?>>();

            result.Value.ShouldBe('1');
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToACharacter()
        {
            var source = new PublicField<ulong> { Value = 87L };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigUnsignedLongToANullableCharacter()
        {
            var source = new PublicField<ulong> { Value = 10 };
            var result = Mapper.Map(source).ToANew<PublicProperty<char?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberFloatOverACharacter()
        {
            var source = new PublicField<float> { Value = 8f };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe('8');
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberNullableFloatToANullableCharacter()
        {
            var source = new PublicProperty<float?> { Value = 7.1f };
            var result = Mapper.Map(source).ToANew<PublicField<char?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberFloatOverACharacter()
        {
            var source = new PublicProperty<float> { Value = 21f };
            var target = Mapper.Map(source).Over(new PublicProperty<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberFloatOverACharacter()
        {
            var source = new PublicProperty<float> { Value = -11f };
            var target = Mapper.Map(source).Over(new PublicProperty<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDecimalToANullableCharacter()
        {
            var source = new PublicGetMethod<decimal>(5.00m);
            var result = Mapper.Map(source).ToANew<PublicProperty<char?>>();

            result.Value.ShouldBe('5');
        }

        [Fact]
        public void ShouldMapAnInRangeNonWholeNumberDecimalOnToACharacter()
        {
            var source = new PublicProperty<decimal> { Value = 6.7m };
            var target = Mapper.Map(source).OnTo(new PublicSetMethod<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapATooBigNonWholeNumberNullableDecimalOverACharacter()
        {
            var source = new PublicProperty<decimal> { Value = 9.1m };
            var target = Mapper.Map(source).Over(new PublicProperty<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnInRangeWholeNumberDoubleToACharacter()
        {
            var source = new PublicField<double> { Value = 2d };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe('2');
        }

        [Fact]
        public void ShouldMapATooBigWholeNumberDoubleOnToANullableCharacter()
        {
            var source = new PublicField<double> { Value = 99d };
            var target = Mapper.Map(source).OnTo(new PublicField<char?>());

            target.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapATooSmallWholeNumberDoubleOverACharacter()
        {
            var source = new PublicProperty<double> { Value = -12d };
            var target = Mapper.Map(source).Over(new PublicSetMethod<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnEnumOverACharacter()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var result = Mapper.Map(source).Over(new PublicProperty<char>());

            result.Value.ShouldBe(((int)Title.Miss).ToString().First());
        }

        [Fact]
        public void ShouldMapANullableEnumToACharacter()
        {
            var source = new PublicField<Title?> { Value = Title.Mr };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBe(((int)Title.Mr).ToString().First());
        }

        [Fact]
        public void ShouldMapANullNullableEnumToACharacter()
        {
            var source = new PublicField<Title?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<char>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapANullableCharacterToACharacter()
        {
            var source = new PublicProperty<char?> { Value = 'h' };
            var result = Mapper.Map(source).ToANew<PublicField<char>>();

            result.Value.ShouldBe('h');
        }

        [Fact]
        public void ShouldMapAParsableStringOnToACharacter()
        {
            var source = new PublicField<string> { Value = "z" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<char>());

            result.Value.ShouldBe('z');
        }

        [Fact]
        public void ShouldMapAParsableStringOverANullableCharacter()
        {
            var source = new PublicField<string> { Value = "B" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<char?>());

            result.Value.ShouldBe('B');
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToACharacter()
        {
            var source = new PublicProperty<string> { Value = "BAGPUSS" };
            var result = Mapper.Map(source).ToANew<PublicField<char>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableNumericStringOverACharacter()
        {
            var source = new PublicField<string> { Value = "67" };
            var target = Mapper.Map(source).Over(new PublicField<char>());

            target.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableCharacter()
        {
            var source = new PublicProperty<string> { Value = "BAGTASTIC" };
            var result = Mapper.Map(source).ToANew<PublicField<char?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapAnObjectIntOverACharacter()
        {
            var source = new PublicProperty<object> { Value = 1 };
            var target = new PublicField<char> { Value = 'x' };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe('1');
        }

        [Fact]
        public void ShouldMapAnObjectParseableStringOnToACharacter()
        {
            var source = new PublicProperty<object> { Value = "9" };
            var target = new PublicField<char> { Value = default(char) };
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe('9');
        }

        [Fact]
        public void ShouldMapANullObjectOverANullableCharacter()
        {
            var source = new PublicProperty<object> { Value = null };
            var target = new PublicField<char?> { Value = '5' };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapANullObjectOnToACharacter()
        {
            var source = new PublicProperty<object> { Value = null };
            var target = new PublicField<char> { Value = '!' };
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe('!');
        }

        [Fact]
        public void ShouldMapAStringEnumerableToACharacterCollection()
        {
            IEnumerable<string> source = new[] { "1", "2", "3" };
            var result = Mapper.Map(source).ToANew<ICollection<char>>();

            result.ShouldBe('1', '2', '3');
        }
    }
}
