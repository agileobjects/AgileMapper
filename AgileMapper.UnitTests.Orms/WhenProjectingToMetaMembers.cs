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

        [Fact]
        public Task ShouldProjectToAHasCollectionMember()
        {
            return RunTest(async context =>
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
            });
        }
    }
}
