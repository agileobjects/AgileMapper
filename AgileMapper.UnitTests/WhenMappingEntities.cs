namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Linq;
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
    }
}
