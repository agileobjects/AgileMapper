namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByValueFilter
    {
        [Fact]
        public void ShouldIgnoreSourceMemberByUntypedValueFilterGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If(value => Equals(value, 123)));

                var source = new PublicTwoFields<int, string> { Value1 = 123, Value2 = "456" };
                var result = mapper.Map(source).ToANew<PublicTwoParamCtor<string, int>>();

                result.ShouldNotBeNull();
                result.Value1.ShouldBeDefault();
                result.Value2.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberByStringValueFilterGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If<string>(str => str == "456"));

                var source = new PublicTwoFields<int, string> { Value1 = 123, Value2 = "456" };
                var result = mapper.Map(source).ToANew<PublicTwoParamCtor<string, int>>();

                result.ShouldNotBeNull();
                result.Value1.ShouldBe("123");
                result.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberArrayElementByIntValueFilterGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If<int>(i => i < 10));

                var source = new PublicField<int[]> { Value = new[] { 1, 7, 11, 15 } };
                var result = mapper.Map(source).ToANew<PublicProperty<ICollection<int>>>();

                result.ShouldNotBeNull();
                result.Value.ShouldNotBeNull();
                result.Value.ShouldBe(11, 15);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberEnumerableElementByStringValueFilterGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If<string>(str => str == "0"));

                var source = new PublicField<IEnumerable<string>> { Value = new[] { "1", "7", "0", "11" } };
                var result = mapper.Map(source).ToANew<PublicProperty<Collection<int>>>();

                result.ShouldNotBeNull();
                result.Value.ShouldNotBeNull();
                result.Value.ShouldBe(1, 7, 11);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMembersByMultiClauseTypedValueFiltersGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c
                        .If<string>(str => str == "123") || c.If<int>(i => i == 123));

                var matchingIntSource = new PublicField<int> { Value = 123 };
                var matchingIntResult = mapper.Map(matchingIntSource).ToANew<PublicProperty<int>>();

                matchingIntResult.ShouldNotBeNull();
                matchingIntResult.Value.ShouldBeDefault();

                var matchingStringSource = new PublicField<string> { Value = "123" };
                var matchingStringResult = mapper.Map(matchingStringSource).ToANew<PublicProperty<string>>();

                matchingStringResult.ShouldNotBeNull();
                matchingStringResult.Value.ShouldBeNull();

                var nonMatchingIntSource = new PublicField<int> { Value = 456 };
                var nonMatchingIntResult = mapper.Map(nonMatchingIntSource).ToANew<PublicProperty<int>>();

                nonMatchingIntResult.ShouldNotBeNull();
                nonMatchingIntResult.Value.ShouldBe(456);

                var nonMatchingStringSource = new PublicField<string> { Value = "999" };
                var nonMatchingStringResult = mapper.Map(nonMatchingStringSource).ToANew<PublicProperty<string>>();

                nonMatchingStringResult.ShouldNotBeNull();
                nonMatchingStringResult.Value.ShouldBe("999");

                var nonMatchingTypeSource = new PublicField<long> { Value = 123L };
                var nonMatchingTypeResult = mapper.Map(nonMatchingTypeSource).ToANew<PublicProperty<string>>();

                nonMatchingTypeResult.ShouldNotBeNull();
                nonMatchingTypeResult.Value.ShouldBe("123");
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberByNullableIntValueFilterGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If<int>(i => i < 10));

                var source = new PublicTwoFields<int, string> { Value1 = 8, Value2 = "456" };
                var result = mapper.Map(source).ToANew<PublicTwoParamCtor<string, int>>();

                result.ShouldNotBeNull();
                result.Value1.ShouldBeNull();
                result.Value2.ShouldBe(456);

                var nonMatchingFilterSource = new PublicTwoFields<int, string> { Value1 = 12, Value2 = "77" };
                var nonMatchingFilterResult = mapper.Map(nonMatchingFilterSource).ToANew<PublicTwoParamCtor<string, long>>();

                nonMatchingFilterResult.ShouldNotBeNull();
                nonMatchingFilterResult.Value1.ShouldBe("12");
                nonMatchingFilterResult.Value2.ShouldBe(77L);

                var nullableSource = new PublicTwoFields<int?, string> { Value1 = 8, Value2 = "99" };
                var nullableResult = mapper.Map(nullableSource).ToANew<PublicTwoParamCtor<string, int>>();

                nullableResult.ShouldNotBeNull();
                nullableResult.Value1.ShouldBeNull();
                nullableResult.Value2.ShouldBe(99);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberByDateTimeValueFilterAndSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<DateTime, string>>()
                    .IgnoreSources(c => c.If<DateTime>(dt => dt < DateTime.Now));

                var anHourAgo = DateTime.Now.AddHours(-1);

                var matchingSource = new PublicTwoFieldsStruct<DateTime, string>
                {
                    Value1 = anHourAgo,
                    Value2 = "456"
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, string>>();

                matchingResult.ShouldNotBeNull();
                matchingResult.Value1.ShouldBeNull();
                matchingResult.Value2.ShouldBe("456");

                var nonMatchingTypeSource = new PublicTwoFieldsStruct<DateTime?, string>
                {
                    Value1 = anHourAgo,
                    Value2 = "123"
                };

                var nonMatchingTypeResult = mapper.Map(nonMatchingTypeSource).ToANew<PublicTwoFields<string, string>>();

                nonMatchingTypeResult.ShouldNotBeNull();
                nonMatchingTypeResult.Value1.ShouldBe(anHourAgo.ToString(CultureInfo.CurrentCulture.DateTimeFormat));
                nonMatchingTypeResult.Value2.ShouldBe("123");

                var nonMatchingFilterSource = new PublicTwoFieldsStruct<DateTime?, string>
                {
                    Value1 = anHourAgo.AddHours(+2),
                    Value2 = "123"
                };

                var nonMatchingFilterResult = mapper.Map(nonMatchingFilterSource).ToANew<PublicTwoFields<string, string>>();

                nonMatchingFilterResult.ShouldNotBeNull();
                nonMatchingFilterResult.Value1.ShouldBe(anHourAgo.AddHours(+2).ToString(CultureInfo.CurrentCulture.DateTimeFormat));
                nonMatchingFilterResult.Value2.ShouldBe("123");
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMemberByNullableLongValueFilterRuleSetSourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<long?, string>>()
                    .ToANew<PublicTwoFields<string, long>>()
                    .IgnoreSources(c => c.If<long?>(l => l.HasValue && l > 100L));

                var matchingSource = new PublicTwoFieldsStruct<long?, string>
                {
                    Value1 = 200L,
                    Value2 = "555"
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, long>>();

                matchingResult.ShouldNotBeNull();
                matchingResult.Value1.ShouldBeNull();
                matchingResult.Value2.ShouldBe(555L);

                var nonMatchingTargetTypeResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, int>>();

                nonMatchingTargetTypeResult.ShouldNotBeNull();
                nonMatchingTargetTypeResult.Value1.ShouldBe("200");
                nonMatchingTargetTypeResult.Value2.ShouldBe(555);

                var nonMatchingRuleSetResult = mapper
                    .Map(matchingSource)
                    .Over(new PublicTwoFields<string, long> { Value1 = "100", Value2 = 55L });

                nonMatchingRuleSetResult.ShouldNotBeNull();
                nonMatchingRuleSetResult.Value1.ShouldBe("200");
                nonMatchingRuleSetResult.Value2.ShouldBe(555L);

                var nullValueSource = new PublicTwoFieldsStruct<int, string>
                {
                    Value1 = 200,
                    Value2 = "444"
                };

                var nullValueResult = mapper.Map(nullValueSource).ToANew<PublicTwoFields<string, long>>();

                nullValueResult.ShouldNotBeNull();
                nullValueResult.Value1.ShouldBe("200");
                nullValueResult.Value2.ShouldBe(444L);

                var nonMatchingSourceTypeSource = new PublicTwoFieldsStruct<int, string>
                {
                    Value1 = 200,
                    Value2 = "444"
                };

                var nonMatchingSourceTypeResult = mapper.Map(nonMatchingSourceTypeSource).ToANew<PublicTwoFields<string, long>>();

                nonMatchingSourceTypeResult.ShouldNotBeNull();
                nonMatchingSourceTypeResult.Value1.ShouldBe("200");
                nonMatchingSourceTypeResult.Value2.ShouldBe(444L);

                var nonMatchingFilterSource = new PublicTwoFieldsStruct<long?, string>
                {
                    Value1 = 99L,
                    Value2 = "123"
                };

                var nonMatchingFilterResult = mapper.Map(nonMatchingFilterSource).ToANew<PublicTwoFields<string, long>>();

                nonMatchingFilterResult.ShouldNotBeNull();
                nonMatchingFilterResult.Value1.ShouldBe("99");
                nonMatchingFilterResult.Value2.ShouldBe(123L);
            }
        }

        [Fact]
        public void ShouldIgnoreConfiguredDataSourceByTimeSpanValueFilterSourceAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<TimeSpan, string>>()
                    .To<PublicField<TimeSpan>>()
                    .Map((ptf, pf) => ptf.Value1)
                    .To(pf => pf.Value)
                    .And
                    .IgnoreSources(c => c.If<TimeSpan>(ts => ts > TimeSpan.FromHours(1)));

                var matchingSource = new PublicTwoFieldsStruct<TimeSpan, string>
                {
                    Value1 = TimeSpan.FromHours(2)
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<TimeSpan>>();

                matchingResult.ShouldNotBeNull();
                matchingResult.Value.ShouldBe(default(TimeSpan));

                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<TimeSpan, string>>()
                    .To<PublicField<TimeSpan?>>()
                    .Map((ptf, pf) => ptf.Value1)
                    .To(pf => pf.Value);

                var nonMatchingTargetTypeResult = mapper.Map(matchingSource).ToANew<PublicField<TimeSpan?>>();

                nonMatchingTargetTypeResult.ShouldNotBeNull();
                nonMatchingTargetTypeResult.Value.ShouldBe(TimeSpan.FromHours(2));

                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<string, string>>()
                    .To<PublicField<TimeSpan>>()
                    .Map((ptf, pf) => ptf.Value1)
                    .To(pf => pf.Value);

                var nonMatchingSourceTypeSource = new PublicTwoFieldsStruct<string, string>
                {
                    Value1 = TimeSpan.FromHours(2).ToString()
                };

                var nonMatchingSourceTypeResult = mapper.Map(nonMatchingSourceTypeSource).ToANew<PublicField<TimeSpan>>();

                nonMatchingSourceTypeResult.ShouldNotBeNull();
                nonMatchingSourceTypeResult.Value.ShouldBe(TimeSpan.FromHours(2));

                var nonMatchingFilterSource = new PublicTwoFieldsStruct<TimeSpan, string>
                {
                    Value1 = TimeSpan.FromMinutes(30)
                };

                var nonMatchingFilterResult = mapper.Map(nonMatchingFilterSource).ToANew<PublicField<TimeSpan>>();

                nonMatchingFilterResult.ShouldNotBeNull();
                nonMatchingFilterResult.Value.ShouldBe(TimeSpan.FromMinutes(30));
            }
        }
    }
}
