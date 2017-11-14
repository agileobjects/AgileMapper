namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using Shouldly;
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

        protected Exception RunTestAndExpectThrow(Action<TOrmContext> testAction)
            => RunTestAndExpectThrow<Exception>(testAction);

        protected TException RunTestAndExpectThrow<TException>(Action<TOrmContext> testAction)
            where TException : Exception
        {
            return Should.Throw<TException>(() => RunTest(testAction));
        }

        protected void RunTest(Action<TOrmContext> testAction)
        {
            try
            {
                testAction.Invoke(Context);
            }
            finally
            {
                EmptyDbContext();
            }
        }

        protected void RunTest(Action<TOrmContext, IMapper> testAction)
        {
            try
            {
                using (var mapper = Mapper.CreateNew())
                {
                    testAction.Invoke(Context, mapper);
                }
            }
            finally
            {
                EmptyDbContext();
            }
        }

        private void EmptyDbContext()
        {
            Context.Products.Clear();
            Context.Addresses.Clear();
            Context.Persons.Clear();
            Context.BoolItems.Clear();
            Context.ShortItems.Clear();
            Context.IntItems.Clear();
            Context.LongItems.Clear();
            Context.StringItems.Clear();
            Context.SaveChanges();
        }
    }
}
