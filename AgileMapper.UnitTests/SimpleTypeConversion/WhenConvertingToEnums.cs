namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConvertingToEnums
    {
        [Fact]
        public void ShouldMapAnIntToAnEnum()
        {
            var source = new PublicProperty<int> { Value = (int)Title.Dr };
            var result = Mapper.Map(source).ToNew<PublicField<Title>>();

            result.Value.ShouldBe((Title)source.Value);
        }

        [Fact]
        public void ShouldMapALongToAnEnum()
        {
            var source = new PublicProperty<long> { Value = (long)Title.Miss };
            var result = Mapper.Map(source).ToNew<PublicField<Title>>();

            result.Value.ShouldBe((Title)source.Value);
        }

        [Fact]
        public void ShouldMapAMatchingStringOnToAnEnum()
        {
            const Title VALUE = Title.Mrs;
            var source = new PublicField<string> { Value = VALUE.ToString() };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAMatchingStringOnToAnEnumCaseInsensitively()
        {
            const Title VALUE = Title.Miss;
            var source = new PublicField<string> { Value = VALUE.ToString().ToLowerInvariant() };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapAMatchingNumericStringOverAnEnum()
        {
            const Title VALUE = Title.Dr;
            var source = new PublicField<string> { Value = ((int)VALUE).ToString() };
            var result = Mapper.Map(source).Over(new PublicProperty<Title>());

            result.Value.ShouldBe(VALUE);
        }

        [Fact]
        public void ShouldMapANonMatchingStringOnToAnEnum()
        {
            var source = new PublicField<string> { Value = "ihdfsjsda" };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBe(default(Title));
        }

        [Fact]
        public void ShouldMapANonMatchingStringToANullableEnum()
        {
            var source = new PublicProperty<string> { Value = "ytej" };
            var result = Mapper.Map(source).ToNew<PublicProperty<Title?>>();

            result.Value.ShouldBeNull();
        }
    }
}
