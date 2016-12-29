namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToDictionaries
    {
        [Fact]
        public void ShouldMapASimpleTypeMemberOnToANullUntypedDictionaryEntry()
        {
            var source = new PublicProperty<long> { Value = long.MaxValue };
            var target = new Dictionary<string, object> { ["Value"] = null };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldHaveSingleItem();
            result["Value"].ShouldBe(long.MaxValue);
        }

        [Fact]
        public void ShouldNotOverwriteAnExistingNonNullUntypedDictionaryEntry()
        {
            var source = new PublicField<short> { Value = short.MinValue };
            var target = new Dictionary<string, object> { ["Value"] = "This is actually a string!" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldHaveSingleItem();
            result["Value"].ShouldBe("This is actually a string!");
        }
    }
}