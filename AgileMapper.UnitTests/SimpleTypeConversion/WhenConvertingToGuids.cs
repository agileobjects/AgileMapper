namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToGuids
    {
        [Fact]
        public void ShouldMapAStringToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<string>(guid.ToString());
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact]
        public void ShouldMapAnObjectGuidToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<object>(guid);
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact]
        public void ShouldMapAnObjectStringGuidToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<object>(guid.ToString());
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact]
        public void ShouldMapAGuidToANullableGuid()
        {
            var source = new PublicGetMethod<Guid>(Guid.NewGuid());
            var result = Mapper.Map(source).ToANew<PublicField<Guid?>>();

            result.Value.ShouldBe(source.GetValue());
        }

        [Fact]
        public void ShouldMapANullableGuidToAGuid()
        {
            var source = new PublicGetMethod<Guid?>(Guid.NewGuid());
            var result = Mapper.Map(source).ToANew<PublicField<Guid>>();

            result.Value.ShouldBe(source.GetValue().GetValueOrDefault());
        }
    }
}
