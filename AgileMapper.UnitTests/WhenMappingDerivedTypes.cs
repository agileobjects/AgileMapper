namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingDerivedTypes
    {
        [Fact]
        public void ShouldMapARootComplexTypeFromItsAssignedType()
        {
            object source = new Product { Price = 100.00 };
            var result = Mapper.Map(source).ToNew<Product>();

            result.Price.ShouldBe(100.00);
        }

        [Fact]
        public void ShouldMapARootComplexTypeEnumerableFromItsAssignedType()
        {
            object source = new[] { new Product { Price = 10.01 } };
            var result = Mapper.Map(source).ToNew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(10.01);
        }

        //[Fact]
        public void ShouldMapARootComplexTypeEnumerableElementFromItsAssignedType()
        {
            var source = new object[] { new Product { Price = 9.99 } };
            var result = Mapper.Map(source).ToNew<IEnumerable<Product>>();

            result.First().Price.ShouldBe(9.99);
        }
    }
}
