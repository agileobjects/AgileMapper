namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverEnumerables
    {
        [Fact]
        public void ShouldOverwriteARootSimpleTypeArray()
        {
            var source = new[] { 5, 4, 3, 2 };
            var target = new[] { 9, 8, 7, 1 };
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldOverwriteARootSimpleTypeList()
        {
            var source = new List<string> { "I", "Will" };
            var target = new List<string> { "You", "Might" };
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldOverwriteARootSimpleTypeEnumerable()
        {
            var source = new List<long> { 234, 567 };
            IEnumerable<long> target = new List<long> { 654, 321 };
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldOverwriteARootComplexTypeList()
        {
            var source = new[]
            {
                new PersonViewModel { Name = "Kate" }
            };

            var target = new List<Person>
            {
                new Person { Name = "Pete" }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.ShouldNotContain(originalObject);
            result.First().Name.ShouldBe("Kate");
        }

        [Fact]
        public void ShouldOverwriteAnExistingObject()
        {
            var source = new[]
            {
                new PersonViewModel { Id = Guid.NewGuid(), Name = "Bart" },
                new PersonViewModel { Id = Guid.NewGuid(), Name = "Lisa" }
            };

            var target = new List<Person>
            {
                new Person { Id = source.Second().Id, Name = "Marge" }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.ShouldContain(originalObject);
            result.SequenceEqual(p => p.Name, "Lisa", "Bart").ShouldBeTrue();
        }

        [Fact]
        public void ShouldHandleANullComplexTypeElement()
        {
            var source = new List<Product>
            {
                null,
                new Product { ProductId = "Swing", Price = 99.99 }
            };

            var target = new List<Product>
            {
                new Product { ProductId = "Swing", Price = 9.99 }
            };

            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeNull();
            result.First().Price.ShouldBe(99.99);
            result.Second().ShouldBeNull();
        }
    }
}
