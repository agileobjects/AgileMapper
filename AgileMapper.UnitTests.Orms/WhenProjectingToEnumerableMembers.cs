namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingToEnumerableMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingToEnumerableMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Project -> Collection

        protected Task RunShouldProjectToAComplexTypeCollectionMember()
            => RunTest(ProjectToComplexTypeCollectionMember);

        protected Task RunShouldErrorProjectingToAComplexTypeCollectionMember()
            => RunTestAndExpectThrow(ProjectToComplexTypeCollectionMember);

        private static async Task ProjectToComplexTypeCollectionMember(TOrmContext context)
        {
            var rotaEntry1 = new RotaEntry
            {
                DayOfWeek = DayOfWeek.Monday,
                PersonId = 10,
                StartHour = 8,
                StartMinute = 45,
                EndHour = 5,
                EndMinute = 15
            };

            var rotaEntry2 = new RotaEntry
            {
                DayOfWeek = DayOfWeek.Tuesday,
                PersonId = 8,
                StartHour = 9,
                StartMinute = 00,
                EndHour = 4,
                EndMinute = 30
            };

            var rotaEntry3 = new RotaEntry
            {
                DayOfWeek = DayOfWeek.Friday,
                PersonId = 51,
                StartHour = 10,
                StartMinute = 30,
                EndHour = 10,
                EndMinute = 31
            };

            var rota = new Rota
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Entries = new List<RotaEntry> { rotaEntry1, rotaEntry2, rotaEntry3 }
            };

            await context.Rotas.Add(rota);
            await context.SaveChanges();

            var rotaDto = context.Rotas.Where(r => r.Id > 0).Project().To<RotaDto>().First();

            rotaDto.Id.ShouldBe(rota.Id);
            rotaDto.StartDate.ShouldBe(rota.StartDate);
            rotaDto.EndDate.ShouldBe(rota.EndDate);
            rotaDto.Entries.Count.ShouldBe(rota.Entries.Count);

            var i = 0;
            var rotaEntryDtos = rotaDto.Entries.OrderBy(re => re.Id).ToArray();

            foreach (var rotaEntry in rota.Entries.OrderBy(re => re.Id))
            {
                var rotaEntryDto = rotaEntryDtos.ElementAt(i);

                rotaEntryDto.Id.ShouldBe(rotaEntry.Id);
                rotaEntryDto.DayOfWeek.ShouldBe(rotaEntry.DayOfWeek);
                rotaEntryDto.PersonId.ShouldBe(rotaEntry.PersonId);
                rotaEntryDto.StartHour.ShouldBe(rotaEntry.StartHour);
                rotaEntryDto.StartMinute.ShouldBe(rotaEntry.StartMinute);
                rotaEntryDto.EndHour.ShouldBe(rotaEntry.EndHour);
                rotaEntryDto.EndMinute.ShouldBe(rotaEntry.EndMinute);

                ++i;
            }
        }

        #endregion

        [Fact]
        public Task ShouldProjectToComplexTypeEnumerableMember()
        {
            return RunTest(async context =>
            {
                var item1 = new OrderItem();
                var item2 = new OrderItem();

                var order = new OrderUk
                {
                    DatePlaced = DateTime.Now,
                    Items = new List<OrderItem> { item1, item2 }
                };

                await context.Orders.Add(order);
                await context.SaveChanges();

                var orderDto = context
                    .Orders
                    .Project()
                    .To<OrderDto>()
                    .ShouldHaveSingleItem();

                orderDto.Id.ShouldBe(order.Id);
                orderDto.DatePlaced.ShouldBe(order.DatePlaced);
                orderDto.Items.Count().ShouldBe(2);

                var i = 0;

                foreach (var orderItem in order.Items)
                {
                    var orderItemDto = order.Items.ElementAt(i);

                    orderItemDto.Id.ShouldBe(orderItem.Id);

                    ++i;
                }
            });
        }

        [Fact]
        public Task ShouldProjectViaLinkingType()
        {
            return RunTest(async context =>
            {
                var account = new Account
                {
                    User = new Person
                    {
                        Name = "Mario",
                        Address = new Address { Line1 = "Here", Postcode = "HS93HS" }
                    }
                };

                account.AddDeliveryAddress(new Address { Line1 = "There", Postcode = "JS95TH" });
                account.AddDeliveryAddress(new Address { Line1 = "Somewhere", Postcode = "KA02ID" });

                await context.Accounts.Add(account);
                await context.SaveChanges();

                var accountDto = context
                    .Accounts
                    .Project()
                    .To<AccountDto>(cfg => cfg
                        .Map(a => a.DeliveryAddresses.OrderBy(da => da.AddressId).Select(da => da.Address))
                        .To(dto => dto.DeliveryAddresses))
                    .ShouldHaveSingleItem();

                accountDto.Id.ShouldBe(account.Id);

                accountDto.User.Id.ShouldBe(account.User.PersonId);
                accountDto.User.Name.ShouldBe(account.User.Name);
                accountDto.User.Address.Id.ShouldBe(account.User.Address.AddressId);
                accountDto.User.Address.Line1.ShouldBe(account.User.Address.Line1);
                accountDto.User.Address.Postcode.ShouldBe(account.User.Address.Postcode);

                accountDto.DeliveryAddresses.Count().ShouldBe(2);

                var addressDto1 = accountDto.DeliveryAddresses.First();
                var address1 = account.DeliveryAddresses.First().Address;
                addressDto1.Id.ShouldBe(address1.AddressId);
                addressDto1.Line1.ShouldBe(address1.Line1);
                addressDto1.Postcode.ShouldBe(address1.Postcode);

                var addressDto2 = accountDto.DeliveryAddresses.Second();
                var address2 = account.DeliveryAddresses.Second().Address;
                addressDto2.Id.ShouldBe(address2.AddressId);
                addressDto2.Line1.ShouldBe(address2.Line1);
                addressDto2.Postcode.ShouldBe(address2.Postcode);
            });
        }
    }
}
