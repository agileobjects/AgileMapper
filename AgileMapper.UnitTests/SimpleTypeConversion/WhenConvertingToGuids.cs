namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToGuids
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAStringToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<string>(guid.ToString());
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectGuidToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<object>(guid);
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectStringGuidToAGuid()
        {
            var guid = Guid.NewGuid();
            var source = new PublicGetMethod<object>(guid.ToString());
            var result = Mapper.Map(source).ToANew<PublicSetMethod<Guid>>();

            result.Value.ShouldBe(guid);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAGuidToANullableGuid()
        {
            var source = new PublicGetMethod<Guid>(Guid.NewGuid());
            var result = Mapper.Map(source).ToANew<PublicField<Guid?>>();

            result.Value.ShouldBe(source.GetValue());
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableGuidToAGuid()
        {
            var source = new PublicGetMethod<Guid?>(Guid.NewGuid());
            var result = Mapper.Map(source).ToANew<PublicField<Guid>>();

            result.Value.ShouldBe(source.GetValue().GetValueOrDefault());
        }
    }
}
