namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenMappingOnToEnumerables
    {
        [Fact]
        public void ShouldMergeARootSimpleTypeArray()
        {
            var source = new[] { 4, 5, 6 };
            var target = new[] { 1, 2, 3 };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull().ShouldBe(1, 2, 3, 4, 5, 6);
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeReadOnlyCollection()
        {
            var source = new[] { 2, 3, 4 };
            var target = new ReadOnlyCollection<string>(new[] { "1", "2" });
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull().ShouldBe("1", "2", "3", "4");
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeList()
        {
            var source = new List<string> { "I", "Will" };
            var target = new List<string> { "Oh", "Heck", "Yes" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target).ShouldBe("Oh", "Heck", "Yes", "I", "Will");
        }

        [Fact]
        public void ShouldMergeARootSimpleTypeCollection()
        {
            var source = new List<string> { "Two", "One" };
            ICollection<string> target = new List<string> { "Four", "Three" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target).ShouldBe("Four", "Three", "Two", "One");
        }

        [Fact]
        public void ShouldExcludeExistingTargetElementsInMerge()
        {
            ICollection<string> source = new List<string> { "Two", "Three" };
            var target = new List<string> { "One", "Two" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target).ShouldBe("One", "Two", "Three");
        }

        [Fact]
        public void ShouldNotExcludeAllExistingTargetElementsInMerge()
        {
            ICollection<string> source = new List<string> { "Two", "Two", "Three" };
            var target = new List<string> { "One", "Two" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target).ShouldBe("One", "Two", "Two", "Three");
        }

        [Fact]
        public void ShouldMergeAComplexTypeList()
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

            result.ShouldBeSameAs(target).ShouldBe(p => p.Name, "Kate", "Pete");
        }

        [Fact]
        public void ShouldMergeASimpleTypeHashSet()
        {
            var source = new[] { 1.0m, 2.0m, 3.0m };
            var target = new HashSet<double> { 2.0, 3.0, 4.0 };

            Mapper.Map(source).OnTo(target);

            target.ShouldBe(2.0, 3.0, 4.0, 1.0);
        }

        [Fact]
        public void ShouldMergeAComplexTypeReadOnlyCollection()
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

            result.ShouldBeSameAs(target).ShouldHaveSingleItem().Name.ShouldBe("Pete");
        }

        [Fact]
        public void ShouldUpdateAnExistingObjectByConvertedId()
        {
            var id = Guid.NewGuid();

            var source = new[]
            {
                new { Id = id.ToString(), Name = "Anonymous" }
            };

            var target = new List<Person>
            {
                new Person { Id = id }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target).ShouldContain(originalObject);
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
