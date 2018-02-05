namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Threading.Tasks;
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

        protected Task<Exception> RunTestAndExpectThrow(Func<TOrmContext, Task> test)
            => RunTestAndExpectThrow<Exception>(test);

        protected Task<TException> RunTestAndExpectThrow<TException>(Func<TOrmContext, Task> test)
            where TException : Exception
        {
            return Should.ThrowAsync<TException>(() => RunTest(test));
        }

        protected async Task RunTest(Func<TOrmContext, Task> test)
        {
            try
            {
                await test.Invoke(Context);
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
                EmptyDbContext();
            }
        }

        private async Task EmptyDbContext()
        {
            Context.Companies.Clear();
            Context.Employees.Clear();
            Context.Categories.Clear();
            Context.Products.Clear();
            Context.Addresses.Clear();
            Context.Persons.Clear();
            Context.Rotas.Clear();
            Context.RotaEntries.Clear();
            Context.BoolItems.Clear();
            Context.ShortItems.Clear();
            Context.IntItems.Clear();
            Context.LongItems.Clear();
            Context.StringItems.Clear();

            await Context.SaveChanges();
        }
    }
}
