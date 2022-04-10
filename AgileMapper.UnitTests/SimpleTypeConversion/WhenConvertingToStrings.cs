namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System;
    using System.Text;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToStrings
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAGuidToAString()
        {
            var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe(source.Value.ToString());
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapADateTimeToAString()
        {
            var source = new PublicProperty<DateTime> { Value = DateTime.Now };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe(source.Value.ToCurrentCultureString());
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAByteToAString()
        {
            var source = new PublicProperty<byte> { Value = 200 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("200");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapASignedByteToAString()
        {
            var source = new PublicProperty<sbyte> { Value = -16 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("-16");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapABase64StringByteArrayToAString()
        {
            var bytes = Encoding.GetEncoding(0).GetBytes("Fish n chips");
            var base64String = Convert.ToBase64String(bytes);
            var base64Bytes = Convert.FromBase64String(base64String);

            var source = new PublicProperty<byte[]> { Value = base64Bytes };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBe(base64String);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullByteArrayToNull()
        {
            var source = new PublicProperty<byte[]> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullByteArrayOverAString()
        {
            var source = new PublicProperty<byte[]> { Value = null };
            var target = new PublicProperty<string> { Value = "Booomtown" };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAShortToAString()
        {
            var source = new PublicProperty<short> { Value = 638 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("638");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntToAString()
        {
            var source = new PublicProperty<int> { Value = 63738 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("63738");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAFloatToAString()
        {
            var source = new PublicProperty<float> { Value = 389.2832f };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("389.2832");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAFloatOnToAnUnpopulatedString()
        {
            var source = new PublicField<float> { Value = 79.22f };
            var target = Mapper.Map(source).OnTo(new PublicProperty<string>());

            target.Value.ShouldBe("79.22");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapALongToAString()
        {
            var source = new PublicProperty<long> { Value = 63738 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("63738");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapALongOnToAPopulatedString()
        {
            var source = new PublicGetMethod<long>(20190738);
            var target = Mapper.Map(source).OnTo(new PublicField<string> { Value = "Boo!" });

            target.Value.ShouldBe("Boo!");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapADecimalToAString()
        {
            var source = new PublicProperty<decimal> { Value = 7638.27282m };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("7638.27282");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableDecimalOverAString()
        {
            var source = new PublicProperty<decimal?> { Value = null };
            var target = Mapper.Map(source).Over(new PublicField<string>());

            target.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapADoubleToAString()
        {
            var source = new PublicProperty<double> { Value = 56473.92 };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("56473.92");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableDoubleToAString()
        {
            var source = new PublicProperty<double?> { Value = 91887.8167473 };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBe("91887.8167473");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableDoubleToAString()
        {
            var source = new PublicProperty<double?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicSetMethod<string>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapACharacterOverAString()
        {
            var source = new PublicField<char> { Value = 'Z' };
            var target = new PublicField<string> { Value = "Nope" };
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe("Z");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableBoolToAString()
        {
            var source = new PublicProperty<bool?> { Value = true };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("true");
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableBoolToAString()
        {
            var source = new PublicProperty<bool?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumOnToAString()
        {
            var source = new PublicField<Title> { Value = Title.Dr };
            var result = Mapper.Map(source).OnTo(new PublicField<string>());

            result.Value.ShouldBe("Dr");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/153
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnInterfaceToAString()
        {
            var source = new { Value = (IPublicInterface<string>)new PublicImplementation<string> { Value = "123" } };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldNotBeNull();
            result.Value.ShouldContain(typeof(PublicImplementation<string>).Name);
        }
    }
}
