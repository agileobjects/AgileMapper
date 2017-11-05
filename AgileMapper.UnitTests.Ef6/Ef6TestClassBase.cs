namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using System;
    using Xunit;

    [Collection("EF6 collection")]
    public abstract class Ef6TestClassBase
    {
        private readonly TestDbContext _context;

        protected Ef6TestClassBase(TestContext context)
        {
            _context = context.DbContext;
        }

        protected void RunTest(Action<TestDbContext> testAction)
        {
            testAction.Invoke(_context);

            EmptyDbContext();
        }

        private void EmptyDbContext()
        {
            _context.Products.RemoveRange(_context.Products);
            _context.BoolItems.RemoveRange(_context.BoolItems);
            _context.ShortItems.RemoveRange(_context.ShortItems);
            _context.IntItems.RemoveRange(_context.IntItems);
            _context.LongItems.RemoveRange(_context.LongItems);
            _context.StringItems.RemoveRange(_context.StringItems);
            _context.SaveChanges();
        }
    }
}
