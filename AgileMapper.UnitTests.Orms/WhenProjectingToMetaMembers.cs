namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        [Fact]
        public Task ShouldProjectToAHasComplexTypeMember()
        {
            return RunTest(async context =>
            {
                var person1 = new Person { Address = new Address { Line1 = "Here!" } };
                var person2 = new Person { Address = default(Address) };

                await context.Persons.AddRange(person1, person2);
                await context.SaveChanges();

                var personDtos = context
                    .Persons
                    .OrderBy(p => p.PersonId)
                    .Project().To<PersonDto>()
                    .ToList();

                personDtos.First().HasAddress.ShouldBeTrue();
                personDtos.Second().HasAddress.ShouldBeFalse();
            });
        }

        #region Project -> HasCollection

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
