namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Common;
    using Xunit;

    [Collection(TestConstants.OrmCollectionName)]
    public abstract class OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected OrmTestClassBase(ITestContext<TOrmContext> context)
        {
            Context = context.DbContext;
        }

        protected TOrmContext Context { get; }

        protected Task<Exception> RunTestAndExpectThrow(Func<TOrmContext, IMapper, Task> test)
            => Should.ThrowAsync(() => RunTest(test));

        protected Task<Exception> RunTestAndExpectThrow(Func<TOrmContext, Task> test)
            => Should.ThrowAsync(() => RunTest(test));

        protected Task RunTest(Func<TOrmContext, IMapper, Task> test)
            => RunTest(mapper => test.Invoke(Context, mapper));

        protected async Task RunTest(Func<TOrmContext, Task> test)
        {
            try
            {
                await test.Invoke(Context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            finally
            {
                await EmptyDbContext();
            }
        }

        protected async Task RunTest(Func<IMapper, Task> test)
        {
            try
            {
                using (var mapper = Mapper.CreateNew())
                {
                    await test.Invoke(mapper);
                }
            }
            finally
            {
                await EmptyDbContext();
            }
        }

        private async Task EmptyDbContext()
        {
            Context.Animals.Clear();
            Context.Shapes.Clear();
            Context.Companies.Clear();
            Context.Employees.Clear();
            Context.Categories.Clear();
            Context.Products.Clear();
            Context.Addresses.Clear();
            Context.Accounts.Clear();
            Context.Persons.Clear();
            Context.OrderItems.Clear();
            Context.Orders.Clear();
            Context.RotaEntries.Clear();
            Context.Rotas.Clear();
            Context.BoolItems.Clear();
            Context.ByteItems.Clear();
            Context.ShortItems.Clear();
            Context.IntItems.Clear();
            Context.NullableIntItems.Clear();
            Context.LongItems.Clear();
            Context.DecimalItems.Clear();
            Context.DoubleItems.Clear();
            Context.DateTimeItems.Clear();
            Context.NullableDateTimeItems.Clear();
            Context.StringItems.Clear();
            Context.TitleItems.Clear();
            Context.NullableTitleItems.Clear();

            await Context.SaveChanges();
        }
    }
}
