namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System;
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
                var order = new Order
                {
                    DatePlaced = DateTime.Now,
                    PaymentType = PaymentTypeUk.Cheque
                };

                context.Orders.Add(order);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Order>()
                        .ProjectedTo<OrderViewModel>()
                        .PairEnum(PaymentTypeUk.Cheque).With(PaymentTypeUs.Check)
                        .And
                        .PairEnum(PaymentTypeUk.Card).With(PaymentTypeUs.Check);

                    var orderVm = context
                        .Orders
                        .ProjectUsing(mapper).To<OrderViewModel>()
                        .ShouldHaveSingleItem();

                    orderVm.DatePlaced.ShouldBe(order.DatePlaced);
                    orderVm.PaymentType.ShouldBe(PaymentTypeUs.Check);
                }
            });
        }
    }
}
