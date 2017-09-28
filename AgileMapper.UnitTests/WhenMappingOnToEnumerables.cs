namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToEnumerables
    {
        [Fact]
        public void ShouldMergeARootSimpleTypeArray()
        {
            var source = new[] { 4, 5, 6 };
            var target = new[] { 1, 2, 3 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull();
            result.ShouldBe(1, 2, 3, 4, 5, 6);
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeReadOnlyCollection()
        {
            var source = new[] { 2, 3, 4 };
            var target = new ReadOnlyCollection<string>(new[] { "1", "2" });
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull();
            result.ShouldBe("1", "2", "3", "4");
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeList()
        {
            var source = new List<string> { "I", "Will" };
            var target = new List<string> { "Oh", "Heck", "Yes" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldBe("Oh", "Heck", "Yes", "I", "Will");
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeCollection()
        {
            var source = new List<string> { "Two", "One" };
            ICollection<string> target = new List<string> { "Four", "Three" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldBe("Four", "Three", "Two", "One");
        }

        [Fact]
        public void ShouldExcludeExistingTargetElementsInMerge()
        {
            ICollection<string> source = new List<string> { "Two", "Three" };
            var target = new List<string> { "One", "Two" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldBe("One", "Two", "Three");
        }

        [Fact]
        public void ShouldNotExcludeAllExistingTargetElementsInMerge()
        {
            ICollection<string> source = new List<string> { "Two", "Two", "Three" };
            var target = new List<string> { "One", "Two" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldBe("One", "Two", "Two", "Three");
        }

        [Fact]
        public void ShouldMergeARootComplexTypeList()
        {
            var source = new[]
            {
                new PersonViewModel { Name = "Pete" }
            };

            var target = new List<Person>
            {
                new Person { Name = "Kate" }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldBe(p => p.Name, "Kate", "Pete");
        }

        [Fact]
        public void ShouldMergeARootComplexTypeReadOnlyCollection()
        {
            var source = new[]
            {
                new Product { ProductId = "Pete" }
            };

            var target = new ReadOnlyCollection<Product>(new[]
            {
                new Product { ProductId = "Kate" }
            });

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBe(p => p.ProductId, "Kate", "Pete");
        }

        [Fact]
        public void ShouldUpdateAnExistingObject()
        {
            var source = new[]
            {
                new PersonViewModel { Id = Guid.NewGuid(), Name = "Pete" }
            };

            var target = new List<Person>
            {
                new Person { Id = source.First().Id }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.First().Name.ShouldBe(source.First().Name);
        }

        [Fact]
        public void ShouldUpdateAnExistingObjectByConvertedId()
        {
            var source = new[]
            {
                new { Id = Guid.NewGuid().ToString(), Name = "Anonymous" }
            };

            var target = new List<Person>
            {
                new Person { Id = Guid.Parse(source.First().Id) }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldContain(originalObject);
            result.First().Name.ShouldBe(source.First().Name);
        }

        [Fact]
        public void ShouldHandleANullSourceIdentifiableElement()
        {
            var source = new List<Product>
            {
                new Product { ProductId = "Bat n ball", Price = 99.99 },
                null
            };

            var target = new List<Product>
            {
                new Product { ProductId = "Bat n ball", Price = 9.99 }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull();
            result.First().Price.ShouldBe(9.99);
            result.Second().ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleANullSourceElementId()
        {
            var source = new List<Product>
            {
                new Product { ProductId = null, Price = 0.99 }
            };

            var target = new List<Product>
            {
                new Product { ProductId = "Bat n ball", Price = 9.99 }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.First().Price.ShouldBe(9.99);
            result.Second().Price.ShouldBe(0.99);
        }

        [Fact]
        public void ShouldHandleANullTargetComplexTypeElement()
        {
            var source = new List<Address>
            {
                new Address { Line1 = "La la la" }
            };

            var target = new List<Address> { null };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull();
            result.First().ShouldBeNull();
            result.Second().Line1.ShouldBe("La la la");
        }
    }
}
