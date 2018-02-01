namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenMappingEntities
    {
        [Fact]
        public void ShouldNotCreateAnEmptyComplexTypeMember()
        {
            var source = new SaveOrderRequest
            {
                Id = 678,
                DateCreated = DateTime.Now,
                Items = new[]
                {
                    new SaveOrderItemRequest { ProductId = 123 },
                    new SaveOrderItemRequest { ProductId = 456 }
                }
            };

            var result = Mapper.Map(source).ToANew<OrderEntity>(cfg => cfg
                .WhenMapping
                .From<SaveOrderItemRequest>()
                .To<OrderItemEntity>()
                .Map(ctx => ctx.Parent.Parent.GetSource<SaveOrderRequest>().Id)
                .To(oi => oi.OrderId));

            result.DateCreated.ShouldBe(source.DateCreated);

            result.Items.Count.ShouldBe(2);

            result.Items.First().OrderId.ShouldBe(678);
            result.Items.First().ProductId.ShouldBe(123);
            result.Items.First().Product.ShouldBeNull();

            result.Items.Second().OrderId.ShouldBe(678);
            result.Items.Second().ProductId.ShouldBe(456);
            result.Items.Second().Product.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotMapEntityId()
        {
            var source = new { Id = 456, ProductSku = Guid.NewGuid() };
            var result = Mapper.Map(source).ToANew<ProductEntity>();

            result.Id.ShouldBeDefault();
            result.ProductSku.ShouldBe(source.ProductSku);
        }

        [Fact]
        public void ShouldPopulateEntityIdWhenCloningAnEntity()
        {
            var source = new ProductEntity { Id = 123, ProductSku = Guid.NewGuid() };
            var result = source.DeepClone();

            result.Id.ShouldBe(123);
            result.ProductSku.ShouldBe(source.ProductSku);
        }

        [Fact]
        public void ShouldMapEntityIdWhenSourceValueIsConfigured()
        {
            var source = new { _Id_ = 456, ProductSku = Guid.NewGuid() };

            var result = source.Map().ToANew<ProductEntity>(cfg => cfg
                .Map((o, p) => o._Id_)
                .To(p => p.Id));

            result.Id.ShouldBe(456);
            result.ProductSku.ShouldBe(source.ProductSku);
        }

        [Fact]
        public void ShouldAutomaticallyShortCircuitRecursionMappingWithATargetDto()
        {
            var order = new OrderEntity
            {
                Id = 123,
                DateCreated = DateTime.Now,
                Items = new List<OrderItemEntity>()
            };

            var product1 = new ProductEntity
            {
                Id = 456,
                ProductSku = Guid.NewGuid(),
                Price = 9.99
            };

            var product2 = new ProductEntity
            {
                Id = 789,
                ProductSku = Guid.NewGuid(),
                Price = 1.99
            };

            order.Items.Add(new OrderItemEntity
            {
                Id = 987,
                OrderId = order.Id,
                Order = order,
                ProductId = product1.Id,
                Product = product1
            });

            order.Items.Add(new OrderItemEntity
            {
                Id = 654,
                OrderId = order.Id,
                Order = order,
                ProductId = product2.Id,
                Product = product2
            });

            var result = Mapper.Map(order).ToANew<OrderDto>(cfg => cfg
                .DisableObjectTracking());

            result.DateCreated.ShouldBe(order.DateCreated);

            result.Items.Count.ShouldBe(2);

            result.Items.First().Id.ShouldBe(987);
            result.Items.First().OrderId.ShouldBe(123);
            result.Items.First().Order.ShouldBeNull();
            result.Items.First().Product.ProductId.ShouldBe(456);
            result.Items.First().Product.Price.ShouldBe(9.99);

            result.Items.Second().Id.ShouldBe(654);
            result.Items.Second().OrderId.ShouldBe(123);
            result.Items.Second().Order.ShouldBeNull();
            result.Items.Second().Product.ProductId.ShouldBe(789);
            result.Items.Second().Product.Price.ShouldBe(1.99);
        }

        [Fact]
        public void ShouldNotMapZeroToAnEntityNullableEntityIdMember()
        {
            var source = new CategoryDto { Name = "Root Category", ParentId = 0 };
            var result = Mapper.Map(source).ToANew<CategoryEntity>();

            result.ParentId.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotMapNullableZeroToAnEntityNullableEntityIdMember()
        {
            var source = new { Name = "Nullable Category", ParentId = (int?)0 };
            var result = Mapper.Map(source).ToANew<CategoryEntity>();

            result.ParentId.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapMinusOneToAnEntityNullableEntityIdMember()
        {
            var source = new CategoryDto { Name = "Negative Category", ParentId = -1 };
            var result = Mapper.Map(source).ToANew<CategoryEntity>();

            result.ParentId.ShouldBe(-1);
        }

        [Fact]
        public void ShouldMapParsedZeroToAnEntityNullableEntityIdMember()
        {
            var source = new { Name = "Parseable Category", ParentId = "0" };
            var result = Mapper.Map(source).ToANew<CategoryEntity>();

            result.ParentId.ShouldBe(0);
        }

        [Fact]
        public void ShouldMapZeroToANonEntityNullableMemberIdMember()
        {
            var source = new CategoryDto { Name = "Root Category Dto", ParentId = 0 };
            var result = source.DeepClone();

            result.ParentId.ShouldBe(0);
        }

        [Fact]
        public void ShouldMapZeroToAnEntityNullableNonEntityIdMember()
        {
            var source = new CategoryDto { Name = "Root Category", ParentId = 0, TopProductIdentifier = 0 };
            var result = source.Map().ToANew<CategoryEntity>();

            result.ParentId.ShouldBeNull();
            result.TopProductIdentifier.ShouldBe(0);
        }
    }
}
