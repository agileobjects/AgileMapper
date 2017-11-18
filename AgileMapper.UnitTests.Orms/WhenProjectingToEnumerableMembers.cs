namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingToEnumerableMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingToEnumerableMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectToAComplexTypeCollectionMember()
        {
            RunTest(context =>
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

                context.Rotas.Add(rota);
                context.SaveChanges();

                var rotaDto = context.Rotas.Where(r => r.Id == 1).ProjectTo<RotaDto>().First();

                rotaDto.Id.ShouldBe(1);
            });
        }
    }
}
