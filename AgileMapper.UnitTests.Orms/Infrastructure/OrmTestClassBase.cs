namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using Shouldly;
    using Xunit;

    [Collection(TestConstants.OrmCollectionName)]
    public abstract class OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        private readonly TOrmContext _context;

        protected OrmTestClassBase(TestContext context)
        {
            _context = context.GetDbContext<TOrmContext>();
        }

        protected Exception RunTestAndExpectThrow(Action<TOrmContext> testAction)
            => RunTestAndExpectThrow<Exception>(testAction);

        protected TException RunTestAndExpectThrow<TException>(Action<TOrmContext> testAction)
            where TException : Exception
        {
            return Should.Throw<TException>(() => RunTest(testAction));
        }

        protected void RunTest(Action<TOrmContext> testAction)
        {
            testAction.Invoke(_context);

            EmptyDbContext();
        }

        private void EmptyDbContext()
        {
            _context.Products.Clear();
            _context.BoolItems.Clear();
            _context.ShortItems.Clear();
            _context.IntItems.Clear();
            _context.LongItems.Clear();
            _context.StringItems.Clear();
            _context.SaveChanges();
        }
    }
}
