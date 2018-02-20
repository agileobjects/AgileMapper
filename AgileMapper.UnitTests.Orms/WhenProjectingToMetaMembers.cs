namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingToMetaMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingToMetaMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Project -> CollectionCount

        protected Task RunShouldProjectToAHasCollectionMember()
            => RunTest(DoShouldProjectToAHasCollectionMember);

        protected Task RunShouldErrorProjectingToAHasCollectionMember()
            => RunTestAndExpectThrow(DoShouldProjectToAHasCollectionMember);

        private static async Task DoShouldProjectToAHasCollectionMember(TOrmContext context)
        {
            var rota = new Rota
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today.AddDays(7),
                Entries = new List<RotaEntry> { new RotaEntry(), new RotaEntry() }
            };

            await context.Rotas.Add(rota);
            await context.SaveChanges();

            var rotaDto = context
                .Rotas
                .Project().To<RotaDto>()
                .ShouldHaveSingleItem();

            rotaDto.HasEntries.ShouldBeTrue();
        }

        #endregion

        [Fact]
        public Task ShouldProjectToAnIntHasEnumerableMember()
        {
            return RunTest(async context =>
            {
                var order = new OrderUk
                {
                    DatePlaced = DateTime.UtcNow,
                    Items = new List<OrderItem> { new OrderItem() }
                };

                await context.Orders.Add(order);
                await context.SaveChanges();

                var orderDto = context
                    .Orders
                    .Project().To<OrderDto>()
                    .ShouldHaveSingleItem();

                orderDto.HasItems.ShouldBe(1);
            });
        }
    }
}
