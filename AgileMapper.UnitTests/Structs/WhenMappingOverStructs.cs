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

        [Fact]
        public void ShouldSetAnExistingSimpleTypePropertyValueToDefault()
        {
            var source = new PublicTwoFields<double?, int>();
            var target = new PublicTwoFieldsStruct<double?, int> { Value1 = 537.0, Value2 = 6382 };

            var result = Mapper.Map(source).Over(target);

            target.Value1.ShouldBe(537.0m);
            target.Value2.ShouldBe(6382);

            result.Value1.ShouldBeNull();
            result.Value2.ShouldBeDefault();
        }
    }
}