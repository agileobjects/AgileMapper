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

            result.Id.ShouldBeDefault();
            result.Discount.ShouldBe(10.00);
        }

        [Fact]
        public void ShouldPreserveAnExistingSimpleTypePropertyValue()
        {
            const long ORIGINAL_VALUE = 527;
            var source = new PublicSealed<long> { Value = 928 };
            var target = new PublicField<long> { Value = ORIGINAL_VALUE };

            Mapper.Map(source).OnTo(target);

            target.Value.ShouldBe(ORIGINAL_VALUE);
        }

        [Fact]
        public void ShouldOverwriteADefaultSimpleTypePropertyValue()
        {
            var source = new PublicGetMethod<decimal>(6372.00m);
            var target = new PublicField<decimal?> { Value = null };

            Mapper.Map(source).OnTo(target);

            target.Value.ShouldBe(source.GetValue());
        }

        [Fact]
        public void ShouldMapOnToASetMethod()
        {
            var source = new PublicGetMethod<double>(5643723);
            var target = new PublicSetMethod<double>();

            Mapper.Map(source).OnTo(target);

            target.Value.ShouldBe(source.GetValue());
        }

        [Fact]
        public void ShouldUseARuntimeTargetType()
        {
            var source = new Customer { Name = "Benji", Discount = 0.2m };
            Person target = new Customer { Name = "Bernard" };
            var result = Mapper.Map(source).OnTo(target);

            ((Customer)result).Discount.ShouldBe(0.2);
        }

        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var target = new PublicProperty<int>();
            var result = Mapper.Map(default(PublicField<int>)).OnTo(target);

            result.ShouldBe(target);
        }

        [Fact]
        public void ShouldHandleATargetWithNoMembers()
        {
            var source = new PublicField<string> { Value = "Nut'in" };
            var target = new object();
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
        }
    }
}
