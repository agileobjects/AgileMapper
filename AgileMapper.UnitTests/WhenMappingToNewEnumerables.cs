namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingToNewEnumerables
    {
        [Fact]
        public void ShouldCreateARootComplexTypeList()
        {
            var source = new List<Person>
            {
                new Person { Name = "Pete", Address = new Address() },
                new Person { Name = "Johnny", Address = new Address() }
            };

            var result = Mapper.Map(source).ToNew<List<Person>>();

            result.ShouldNotBeNull();
            result.ShouldNotBe(source);
            result.First().ShouldNotBe(source.First());
            result.First().Name.ShouldBe(source.First().Name);
            result.Last().ShouldNotBe(source.Last());
            result.Last().Name.ShouldBe(source.Last().Name);
        }

        [Fact]
        public void ShouldHandleANullComplexTypeElement()
        {
            var source = new List<Product>
            {
                new Product { ProductId = "Surprise" },
                null,
                new Product { ProductId = "Boomstick" }
            };

            var result = Mapper.Map(source).ToNew<List<Product>>();

            result.ShouldNotBeNull();
            result.ShouldNotBe(source);
            result.Second().ShouldBeNull();
        }
    }
}
