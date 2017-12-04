namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverEnumerables
    {
        [Fact]
        public void ShouldOverwriteARootSimpleTypeArray()
        {
            var source = new[] { '5', '4', '3', '2' };
            var target = new[] { 9, 8, 7, 1 };
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeNull();
            result.ShouldNotBeSameAs(source);
            result.ShouldBe(5, 4, 3, 2);
        }

        [Fact]
        public void ShouldOverwriteARootSimpleTypeReadOnlyCollection()
        {
            var source = new[] { '2', '3' };
            var target = new ReadOnlyCollection<char>(new List<char> { '5', '4' });
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeNull();
            result.ShouldBe('2', '3');
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
        public void ShouldOverwriteARootComplexTypeCollectionElementByRuntimeType()
        {
            var id = Guid.NewGuid();

            var source = new[]
            {
                new CustomerViewModel { Id = id, Name = "Kate", Discount = 0.2 }
            };

            var target = new Collection<Person>
            {
                new Customer { Id = id, Name = "George" }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.ShouldContain(originalObject);
            result.ShouldHaveSingleItem();
            result.First().Name.ShouldBe("Kate");
            ((Customer)result.First()).Discount.ShouldBe(0.2);
        }

        [Fact]
        public void ShouldOverwriteAnExistingObjectById()
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
            result.ShouldBe(p => p.Name, "Lisa", "Bart");
        }

        [Fact]
        public void ShouldOverwriteAnExistingObjectByIdInAReadOnlyCollection()
        {
            var source = new[]
            {
                new CustomerViewModel { Id = Guid.NewGuid(), Name = "Homer" }
            };

            var target = new ReadOnlyCollection<CustomerViewModel>(new List<CustomerViewModel>
            {
                new CustomerViewModel { Id = source.First().Id, Name = "Maggie" }
            });

            var originalObject = target.First();
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBeSameAs(target);
            result.ShouldHaveSingleItem();
            result.First().ShouldBeSameAs(originalObject);
            result.First().Name.ShouldBe("Homer");
        }

        [Fact]
        public void ShouldOverwriteAnIReadOnlyCollectionList()
        {
            var source = new[]
            {
                new ProductDto { ProductId = "khujygtf", Price = 0.75m }
            };

            IReadOnlyCollection<ProductDto> target = new List<ProductDto>
            {
                new ProductDto { ProductId = "kjsdkljnax", Price = 0.99m }
            };

            var originalObject = target.First();
            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.ShouldHaveSingleItem();
            result.First().ShouldNotBeSameAs(originalObject);
            result.First().ProductId.ShouldBe("khujygtf");
            result.First().Price.ShouldBe(0.75m);
        }

        [Fact]
        public void ShouldOverwriteUsingAConfiguredDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<PublicField<string>>()
                    .Map((p, pf) => p.ProductId)
                    .To(pf => pf.Value);

                var source = new[]
                {
                    new Product { ProductId = "Bart" },
                    new Product { ProductId = "Lisa" }
                };

                var target = new Collection<PublicField<string>>
                {
                    new PublicField<string> { Value = "Homer" },
                    new PublicField<string> { Value = "Marge" }
                };

                var originalObjectOne = target.First();
                var originalObjectTwo = target.Second();

                mapper.Map(source).Over(target);

                target.ShouldNotContain(originalObjectOne);
                target.ShouldNotContain(originalObjectTwo);
                target.ShouldBe(p => p.Value, "Bart", "Lisa");
            }
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

        [Fact]
        public void ShouldHandleANonEnumerableSource()
        {
            var target = new List<Product>
            {
                new Product { ProductId = "Swing", Price = 9.99 }
            };

            var result = Mapper.Map(new PublicProperty<double> { Value = 99.99 }).Over(target);

            result.ShouldBeSameAs(target);
        }
    }
}
