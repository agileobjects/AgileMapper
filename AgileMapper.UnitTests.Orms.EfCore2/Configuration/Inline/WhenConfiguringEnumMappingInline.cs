namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration.Inline
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;
    using PaymentTypeUk = UnitTests.TestClasses.PaymentTypeUk;
    using PaymentTypeUs = UnitTests.TestClasses.PaymentTypeUs;

    public class WhenConfiguringEnumMappingInline : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenConfiguringEnumMappingInline(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldPairEnumMembersInline()
        {
            return RunTest(async (context, mapper) =>
            {
                var order1 = new OrderUk
                {
                    DatePlaced = DateTime.Now,
                    PaymentType = PaymentTypeUk.Cheque
                };

                var order2 = new OrderUk
                {
                    DatePlaced = DateTime.Now.AddMinutes(1),
                    PaymentType = PaymentTypeUk.Cash
                };

                var order3 = new OrderUk
                {
                    DatePlaced = DateTime.Now.AddMinutes(2),
                    PaymentType = PaymentTypeUk.Card
                };

                await context.Orders.AddRangeAsync(order1, order2, order3);
                await context.SaveChangesAsync();

                var orderVms = await context
                    .Orders
                    .ProjectUsing(mapper).To<OrderUsViewModel>(cfg => cfg
                        .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check))
                    .OrderByDescending(o => o.DatePlaced)
                    .ToArrayAsync();

                orderVms.Length.ShouldBe(3);

                orderVms.First().DatePlaced.ShouldBe(order3.DatePlaced);
                orderVms.First().PaymentType.ShouldBe(PaymentTypeUs.Card);

                orderVms.Second().DatePlaced.ShouldBe(order2.DatePlaced);
                orderVms.Second().PaymentType.ShouldBe(PaymentTypeUs.Cash);

                orderVms.Third().DatePlaced.ShouldBe(order1.DatePlaced);
                orderVms.Third().PaymentType.ShouldBe(PaymentTypeUs.Check);
            });
        }
    }
}
