namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;

    public class EfCore1DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public EfCore1DbSetWrapper(DbSet<TEntity> dbSet)
            : base(dbSet)
        {
            _dbSet = dbSet;
        }

        public override void Add(TEntity itemToAdd) => _dbSet.Add(itemToAdd);

        public override void Clear() => _dbSet.RemoveRange(_dbSet);
    }
}