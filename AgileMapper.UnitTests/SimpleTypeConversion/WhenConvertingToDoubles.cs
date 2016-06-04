namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using System.Collections.ObjectModel;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToDoubles
    {
        [Fact]
        public void ShouldMapASignedByteOverADouble()
        {
            var source = new PublicProperty<sbyte> { Value = 83 };
            var result = Mapper.Map(source).Over(new PublicField<double> { Value = 64738 });

            result.Value.ShouldBe(83);
        }

        [Fact]
        public void ShouldMapAByteOntoADouble()
        {
            var source = new PublicProperty<byte> { Value = 99 };
            var result = Mapper.Map(source).OnTo(new PublicField<double>());

            result.Value.ShouldBe(99);
        }

        [Fact]
        public void ShouldMapAShortToADouble()
        {
            var source = new PublicProperty<short> { Value = 9287 };
            var result = Mapper.Map(source).ToNew<PublicField<double>>();

            result.Value.ShouldBe(9287);
        }

        [Fact]
        public void ShouldMapAnIntToADouble()
        {
            var source = new PublicField<int> { Value = 32156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(32156);
        }

        [Fact]
        public void ShouldMapAnUnsignedIntToADouble()
        {
            var source = new PublicField<uint> { Value = 32658 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(32658);
        }
        [Fact]
        public void ShouldMapALongToADouble()
        {
            var source = new PublicField<long> { Value = 3156 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(3156);
        }

        [Fact]
        public void ShouldMapAnUnsignedLongToADouble()
        {
            var source = new PublicField<ulong> { Value = 9292726 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(9292726);
        }

        [Fact]
        public void ShouldMapAnUnsignedLongToANullableDouble()
        {
            var source = new PublicField<ulong> { Value = 9383625 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double?>>();

            result.Value.ShouldBe(9383625);
        }

        [Fact]
        public void ShouldMapAWholeNumberFloatOverADouble()
        {
            var source = new PublicField<float> { Value = 8532.00f };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(8532);
        }

        [Fact]
        public void ShouldMapANonWholeNumberNullableFloatToANullableDouble()
        {
            var source = new PublicProperty<float?> { Value = 73.62f };
            var result = Mapper.Map(source).ToNew<PublicField<double?>>();

            Math.Round(result.Value.GetValueOrDefault(), 2).ShouldBe(73.62);
        }

        [Fact]
        public void ShouldMapAWholeNumberDecimalToADouble()
        {
            var source = new PublicField<decimal> { Value = 637128 };
            var result = Mapper.Map(source).ToNew<PublicProperty<double>>();

            result.Value.ShouldBe(637128);
        }

        [Fact]
        public void ShouldMapAWholeNumberDecimalToANullableDouble()
        {
            var source = new PublicGetMethod<decimal>(5332.00m);
            var result = Mapper.Map(source).ToNew<PublicProperty<double?>>();

            result.Value.ShouldBe(5332);
        }

        [Fact]
        public void ShouldMapANonWholeNumberNullableDecimalOverADouble()
        {
            var source = new PublicProperty<decimal?> { Value = 938378.637m };
            var target = Mapper.Map(source).Over(new PublicProperty<double>());

            target.Value.ShouldBe(938378.637);
        }

        [Fact]
        public void ShouldMapAnEnumOverADouble()
        {
            var source = new PublicField<Title> { Value = Title.Miss };
            var target = Mapper.Map(source).Over(new PublicProperty<double>());

            target.Value.ShouldBe((double)Title.Miss);
        }

        [Fact]
        public void ShouldMapACharacterToANullableDouble()
        {
            var source = new PublicProperty<char> { Value = '9' };
            var result = Mapper.Map(source).ToNew<PublicField<double?>>();

            result.Value.ShouldBe(9);
        }

        [Fact]
        public void ShouldMapAnUnparsableCharacterToADouble()
        {
            var source = new PublicProperty<char> { Value = 'l' };
            var result = Mapper.Map(source).ToNew<PublicField<double>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAParsableWholeNumberStringOnToADouble()
        {
            var source = new PublicField<string> { Value = "6347687" };
            var result = Mapper.Map(source).OnTo(new PublicSetMethod<double>());

            result.Value.ShouldBe(6347687);
        }

        [Fact]
        public void ShouldMapAParsableNonWholeNumberStringOverANullableDouble()
        {
            var source = new PublicProperty<string> { Value = "6372389.63" };
            var result = Mapper.Map(source).Over(new PublicSetMethod<double?>());

            result.Value.ShouldBe(6372389.63);
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToADouble()
        {
            var source = new PublicProperty<string> { Value = "TURKEY" };
            var result = Mapper.Map(source).ToNew<PublicField<double>>();

            result.Value.ShouldBeDefault();
        }

        [Fact]
        public void ShouldMapAnUnparsableStringToANullableDouble()
        {
            var source = new PublicProperty<string> { Value = "CHEETOS" };
            var result = Mapper.Map(source).ToNew<PublicField<double?>>();

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapACharacterArrayOnToADoubleCollection()
        {
            var source = new[] { '7', '2', '7' };
            var target = new Collection<double> { 2, 7 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(target);
            result.ShouldBe(2, 7, 7);
        }
    }
}
