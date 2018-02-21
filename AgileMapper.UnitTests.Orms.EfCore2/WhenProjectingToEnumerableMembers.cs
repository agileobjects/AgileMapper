namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using TestClasses;
    using Xunit;

    public class WhenProjectingToEnumerableMembers : WhenProjectingToEnumerableMembers<EfCore2TestDbContext>
    {
        public WhenProjectingToEnumerableMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectToAComplexTypeCollectionMember()
            => RunShouldProjectToAComplexTypeCollectionMember();

        [Fact]
        public Task ShouldMaterialiseEnumerableMembers()
        {
            return RunTest(async context =>
            {
                var order = new OrderUk
                {
                    DatePlaced = DateTime.Now.AddHours(-2),
                    Items = new List<OrderItem> { new OrderItem { ProductName = "Monster Feet" } }
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                var orderDto = await context
                    .Orders
                    .Project().To<OrderDto>()
                    .FirstAsync();

                orderDto.DatePlaced.ShouldBe(order.DatePlaced);
                orderDto.HasItems.ShouldBe(1);
                orderDto.Items.ShouldBeOfType<List<OrderItemDto>>();
            });
        }
    }
}