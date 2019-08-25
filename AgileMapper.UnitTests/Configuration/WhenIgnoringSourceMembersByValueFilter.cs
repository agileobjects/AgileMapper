namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
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
        public void ShouldIgnoreSourceMemberByStringTypedValueFilterGlobally()
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
    }
}
