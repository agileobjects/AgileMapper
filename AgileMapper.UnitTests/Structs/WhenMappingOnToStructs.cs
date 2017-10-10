﻿namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToStructs
    {
        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Value1 = "One", Value2 = 10.00m };
            var target = new PublicTwoFieldsStruct<string, string> { Value1 = "Zero" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBe(target);
            result.Value1.ShouldBe("Zero");
            result.Value2.ShouldBe("10.00");
        }

        [Fact]
        public void ShouldPreserveAnExistingSimpleTypePropertyValue()
        {
            var source = new PublicPropertyStruct<long> { Value = 928 };
            var target = new PublicPropertyStruct<long> { Value = 527 };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe(527);
        }

        [Fact]
        public void ShouldOverwriteADefaultSimpleTypePropertyValue()
        {
            var source = new PublicGetMethod<decimal>(6372.00m);
            var target = new PublicPropertyStruct<decimal?> { Value = null };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBe(6372.00m);
        }
    }
}