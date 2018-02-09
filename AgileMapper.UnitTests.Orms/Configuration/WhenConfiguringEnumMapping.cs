namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;
    using PaymentTypeUk = UnitTests.TestClasses.PaymentTypeUk;
    using PaymentTypeUs = UnitTests.TestClasses.PaymentTypeUs;

    public abstract class WhenConfiguringEnumMapping<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringEnumMapping(ITestContext<TOrmContext> context) : base(context)
        {
        }

        [Fact]
        public Task ShouldPairEnumMembers()
        {
            return RunTest(async context =>
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

                context.Orders.Add(order1);
                context.Orders.Add(order2);
                context.Orders.Add(order3);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<OrderUk>()
                        .ProjectedTo<OrderUsViewModel>()
                        .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check)
                        .And
                        .PairEnum(PaymentTypeUk.Card).With(PaymentTypeUs.Check);

                    var orderVms = context
                        .Orders
                        .ProjectUsing(mapper).To<OrderUsViewModel>()
                        .OrderBy(o => o.DatePlaced)
                        .ToArray();

                    orderVms.Length.ShouldBe(3);

                    orderVms.First().DatePlaced.ShouldBe(order1.DatePlaced);
                    orderVms.First().PaymentType.ShouldBe(PaymentTypeUs.Check);

                    orderVms.Second().DatePlaced.ShouldBe(order2.DatePlaced);
                    orderVms.Second().PaymentType.ShouldBe(PaymentTypeUs.Cash);

                    orderVms.Third().DatePlaced.ShouldBe(order3.DatePlaced);
                    orderVms.Third().PaymentType.ShouldBe(PaymentTypeUs.Check);
                }
            });
        }
    }
}
