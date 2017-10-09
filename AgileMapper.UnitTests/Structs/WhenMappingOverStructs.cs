namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverStructs
    {
        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Value1 = Guid.NewGuid(), Value2 = "Mr Pants" };
            var target = new PublicTwoFieldsStruct<Guid, string>()
            {
                Value1 = Guid.NewGuid(),
                Value2 = "Mrs Trousers"
            };
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeSameAs(target);
            result.Value1.ShouldBe(source.Value1);
            result.Value2.ShouldBe(source.Value2);
        }
    }
}