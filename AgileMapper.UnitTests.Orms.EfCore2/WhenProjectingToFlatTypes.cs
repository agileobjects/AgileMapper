namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using TestClasses;
    using Xunit;

    public class WhenProjectingToFlatTypes : WhenProjectingToFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingToFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectUsingEfFunctionsLike()
        {
            return RunTest(async context =>
            {
                var person1 = new Person { Name = "Person One" };
                var person2 = new Person { Name = "Person Two" };
                var person3 = new Person { Name = "Person Three" };

                await context.Persons.AddRangeAsync(person1, person2, person3);
                await context.SaveChangesAsync();

                var personVms = await context
                    .Persons
                    .Where(pvm => EF.Functions.Like(pvm.Name, "%Tw%"))
                    .Project().To<PersonViewModel>()
                    .OrderBy(pvm => pvm.Id)
                    .ToListAsync();

                var personVm = personVms.ShouldHaveSingleItem();

                personVm.Id.ShouldBe(person2.PersonId);
                personVm.Name.ShouldBe("Person Two");
                personVm.AddressId.ShouldBeNull();
            });
        }

        [Fact]
        public Task ShouldProjectUsingDatePart()
        {
            return RunTest(async context =>
            {
                var dateTime1 = new PublicDateTime { Value = DateTime.Today.AddMonths(-1) };
                var dateTime2 = new PublicDateTime { Value = DateTime.Today };
                var dateTime3 = new PublicDateTime { Value = DateTime.Today.AddMonths(+1) };

                await context.DateTimeItems.AddRangeAsync(dateTime1, dateTime2, dateTime3);
                await context.SaveChangesAsync();

                var dateTimeDtos = await context
                    .DateTimeItems
                    .Where(d => d.Value.Month == DateTime.Today.Month)
                    .Project().To<PublicDateTimeDto>()
                    .OrderBy(pvm => pvm.Id)
                    .ToListAsync();

                var dateTimeDto = dateTimeDtos.ShouldHaveSingleItem();

                dateTimeDto.Id.ShouldBe(dateTime2.Id);
                dateTimeDto.Value.ShouldBe(DateTime.Today);
            });
        }

        [Fact]
        public Task ShouldProjectUsingDateDiff()
        {
            return RunTest(async context =>
            {
                var dateTime1 = new PublicDateTime { Value = DateTime.Today.AddMonths(-1) };
                var dateTime2 = new PublicDateTime { Value = DateTime.Today };
                var dateTime3 = new PublicDateTime { Value = DateTime.Today.AddMonths(+1) };

                await context.DateTimeItems.AddRangeAsync(dateTime1, dateTime2, dateTime3);
                await context.SaveChangesAsync();

                var dateTimeDtos = await context
                    .DateTimeItems
                    .Where(d => (d.Value - DateTime.Today).TotalDays > 15)
                    .Project().To<PublicDateTimeDto>()
                    .OrderBy(pvm => pvm.Id)
                    .ToListAsync();

                var dateTimeDto = dateTimeDtos.ShouldHaveSingleItem();

                dateTimeDto.Id.ShouldBe(dateTime3.Id);
                dateTimeDto.Value.ShouldBe(dateTime3.Value);
            });
        }
    }
}
