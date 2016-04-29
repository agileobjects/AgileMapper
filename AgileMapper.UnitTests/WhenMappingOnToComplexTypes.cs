namespace AgileObjects.AgileMapper.UnitTests
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToComplexTypes
    {
        [Fact]
        public void ShouldReuseAnExistingTargetObject()
        {
            var source = new PublicField<string>();
            var target = new PublicProperty<string>();

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(target);
        }

        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Discount = 10.00m };
            var target = new Customer { Name = "Captain Customer" };
            var result = Mapper.Map(source).OnTo(target);

            result.Id.ShouldBe(target.Id);
            result.Discount.ShouldBe(source.Discount);
        }

        [Fact]
        public void ShouldPreserveAnExistingSimpleTypePropertyValue()
        {
            const long ORIGINAL_VALUE = 527;
            var source = new PublicProperty<long> { Value = 928 };
            var target = new PublicField<long> { Value = ORIGINAL_VALUE };

            Mapper.Map(source).OnTo(target);

            target.Value.ShouldBe(ORIGINAL_VALUE);
        }
    }
}
