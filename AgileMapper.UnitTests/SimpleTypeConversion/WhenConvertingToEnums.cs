namespace AgileObjects.AgileMapper.UnitTests.SimpleTypeConversion
{
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
    using static Common.TestClasses.Title;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConvertingToEnums
    {
        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAByteToAnEnum()
        {
            var source = new PublicField<byte> { Value = (byte)Dr };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            result.Value.ShouldBe(Dr);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAShortToAnEnum()
        {
            var source = new PublicField<short> { Value = (short)Miss };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            result.Value.ShouldBe(Miss);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableIntToAnEnum()
        {
            var source = new PublicProperty<int?> { Value = (int)Lady };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            result.Value.ShouldBe(Lady);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnIntToAnEnum()
        {
            var source = new PublicProperty<int> { Value = (int)Dr };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            result.Value.ShouldBe((Title)source.Value);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableIntToAnEnum()
        {
            var source = new PublicProperty<int?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicField<TitleShortlist>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapALongToAnEnum()
        {
            var source = new PublicProperty<long> { Value = (long)Miss };
            var result = Mapper.Map(source).ToANew<PublicField<Title>>();

            result.Value.ShouldBe((Title)source.Value);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonMatchingNullableLongToANullableEnum()
        {
            var source = new PublicProperty<long?> { Value = (long)Earl };
            var result = Mapper.Map(source).ToANew<PublicField<TitleShortlist?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingCharacterOnToAnEnum()
        {
            var source = new PublicField<char> { Value = '6' };
            var result = Mapper.Map(source).OnTo(new PublicProperty<TitleShortlist>());

            result.Value.ShouldBe((TitleShortlist)6);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonMatchingNullableCharacterOnToANullableEnum()
        {
            var source = new PublicField<char?> { Value = 'x' };
            var result = Mapper.Map(source).OnTo(new PublicProperty<TitleShortlist?>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingNullableCharacterOnToANullableEnum()
        {
            var source = new PublicField<char?> { Value = '2' };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title?>());

            result.Value.ShouldBe((Title)2);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingStringOnToAnEnum()
        {
            var source = new PublicField<string> { Value = Mrs.ToString() };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBe(Mrs);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingStringOnToAnEnumCaseInsensitively()
        {
            var source = new PublicField<string> { Value = Miss.ToString().ToLowerInvariant() };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBe(Miss);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingStringOnToARootEnum()
        {
            var result = Mapper.Map(Mrs.ToString()).ToANew<Title>();

            result.ShouldBe(Mrs);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAMatchingNumericStringOverAnEnum()
        {
            var source = new PublicField<string> { Value = ((int)Dr).ToString() };
            var result = Mapper.Map(source).Over(new PublicProperty<Title>());

            result.Value.ShouldBe(Dr);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonMatchingStringOnToAnEnum()
        {
            var source = new PublicField<string> { Value = "ihdfsjsda" };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonMatchingStringToANullableEnum()
        {
            var source = new PublicProperty<string> { Value = "ytej" };
            var result = Mapper.Map(source).ToANew<PublicProperty<Title?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullStringOverANullableEnum()
        {
            var source = new PublicField<string> { Value = default(string) };
            var target = new PublicProperty<Title?> { Value = Dr };

            Mapper.Map(source).Over(target);

            target.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEmptyStringOnToAnEnum()
        {
            var source = new PublicField<string> { Value = string.Empty };
            var result = Mapper.Map(source).OnTo(new PublicProperty<Title>());

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumToAnEnum()
        {
            var source = new PublicProperty<TitleShortlist> { Value = TitleShortlist.Mrs };
            var result = Mapper.Map(source).ToANew<PublicProperty<Title>>();

            result.Value.ShouldBe(Mrs);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnEnumToARootEnum()
        {
            var result = Mapper.Map(TitleShortlist.Mrs).ToANew<Title>();

            result.ShouldBe(Mrs);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANonMatchingEnumToANullableEnum()
        {
            var source = new PublicProperty<Title> { Value = Lord };
            var result = Mapper.Map(source).ToANew<PublicProperty<TitleShortlist?>>();

            result.Value.ShouldBeNull();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullableEnumToAnEnum()
        {
            var source = new PublicProperty<Title?> { Value = Dr };
            var result = Mapper.Map(source).ToANew<PublicProperty<Title>>();

            result.Value.ShouldBe(Dr);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullNullableEnumToAnEnum()
        {
            var source = new PublicProperty<Title?> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<Title>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapANullObjectToAnEnum()
        {
            var source = new PublicProperty<object> { Value = null };
            var result = Mapper.Map(source).ToANew<PublicProperty<Title>>();

            result.Value.ShouldBeDefault();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectEnumMemberNameToAnEnum()
        {
            var source = new PublicProperty<object> { Value = "Ms" };
            var result = Mapper.Map(source).ToANew<PublicProperty<TitleShortlist>>();

            result.Value.ShouldBe(TitleShortlist.Ms);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectEnumMemberValueToAnNullableEnum()
        {
            var source = new PublicProperty<object> { Value = (int)TitleShortlist.Dr };
            var result = Mapper.Map(source).ToANew<PublicProperty<TitleShortlist>>();

            result.Value.ShouldBe(TitleShortlist.Dr);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapAnObjectNullableEnumMemberValueToAnNullableEnum()
        {
            var source = new PublicProperty<object> { Value = (Title?)Mr };
            var result = Mapper.Map(source).ToANew<PublicProperty<TitleShortlist>>();

            result.Value.ShouldBe(TitleShortlist.Mr);
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapEnumsConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Title?, Title>>()
                    .To<PublicField<TitleShortlist>>()
                    .Map((ptf, pf) => ptf.Value1).To(pf => pf.Value)
                    .But
                    .If((ptf, pf) => ptf.Value1 == null)
                    .Map((ptf, pf) => ptf.Value2).To(pf => pf.Value)
                    .And
                    .If((ptf, pf) => Duke == ptf.Value1)
                    .Map(TitleShortlist.Other).To(pf => pf.Value);

                var nonNullSource = new PublicTwoFields<Title?, Title> { Value1 = Dr, Value2 = Count };
                var nonNullResult = mapper.Map(nonNullSource).ToANew<PublicField<TitleShortlist>>();

                nonNullResult.Value.ShouldBe(TitleShortlist.Dr);

                var nullSource = new PublicTwoFields<Title?, Title> { Value1 = null, Value2 = Mrs };
                var nullResult = mapper.Map(nullSource).ToANew<PublicField<TitleShortlist>>();

                nullResult.Value.ShouldBe(TitleShortlist.Mrs);

                var dukeSource = new PublicTwoFields<Title?, Title> { Value1 = Duke };
                var dukeResult = mapper.Map(dukeSource).ToANew<PublicField<TitleShortlist>>();

                dukeResult.Value.ShouldBe(TitleShortlist.Other);
            }
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldMapNonIntDerivedRootEnumValuesInAnArray()
        {
            var source = new[] { (short)ShortNumbers.One, (short)ShortNumbers.Three, (short)5 };

            var result = Mapper.Map(source).ToANew<IList<ShortNumbers>>();

            // 5 isn't a valid value, so it should default to default(ShortNumbers):
            result.ShouldBe(ShortNumbers.One, ShortNumbers.Three, ShortNumbers.Zero);
        }

        #region Helper Members

        public enum ShortNumbers : short
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3
        }

        #endregion
    }
}
