namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByValueFilterInline
    {
        [Fact]
        public void ShouldIgnoreSourceMembersByMultiClauseTypedValueFiltersOnline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingIntResult = mapper
                    .Map(new PublicField<int> { Value = 123 })
                    .ToANew<PublicProperty<int>>(cfg => cfg
                        .IgnoreSources(c => 
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                matchingIntResult.ShouldNotBeNull();
                matchingIntResult.Value.ShouldBeDefault();

                var matchingStringResult = mapper
                    .Map(new PublicField<string> { Value = "123" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c => 
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                matchingStringResult.ShouldNotBeNull();
                matchingStringResult.Value.ShouldBeNull();

                var nonMatchingIntResult = mapper
                    .Map(new PublicField<int> { Value = 456 })
                    .ToANew<PublicProperty<int>>(cfg => cfg
                        .IgnoreSources(c => 
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingIntResult.ShouldNotBeNull();
                nonMatchingIntResult.Value.ShouldBe(456);

                var nonMatchingStringResult = mapper
                    .Map(new PublicField<string> { Value = "999" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c => 
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                            (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingStringResult.ShouldNotBeNull();
                nonMatchingStringResult.Value.ShouldBe("999");

                var nonMatchingTypeResult = mapper
                    .Map(new PublicField<long> { Value = 123L })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c => 
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                            (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingTypeResult.ShouldNotBeNull();
                nonMatchingTypeResult.Value.ShouldBe("123");
            }
        }

        [Fact]
        public void ShouldExtendSourceMemberValueFilterConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, long>>()
                    .Over<PublicTwoFields<long, long>>()
                    .IgnoreSources(c => c.If<int>(i => i < 10));

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 4, Value2 = 12L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<long>(l => l > 10L)));

                result1.Value1.ShouldBeDefault(); // int < 10
                result1.Value2.ShouldBeDefault(); // long > 10

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 20, Value2 = 15L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<long>(l => l > 10L)));

                result2.Value1.ShouldBe(20);
                result2.Value2.ShouldBeDefault(); // long > 10

                mapper.InlineContexts().ShouldHaveSingleItem();

                var result3 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 20, Value2 = 11L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<int>(i => i < 25))
                        .And
                        .IgnoreSources(c => c.If<long>(l => l > 12L)));

                result3.Value1.ShouldBeDefault(); // int < 25
                result3.Value2.ShouldBe(11);

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }
    }
}
