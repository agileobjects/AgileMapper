namespace AgileObjects.AgileMapper.IntegrationTests.Orms.Ef6.Infrastructure
{
    using System.Data.Entity;
    using UnitTests.Orms.Infrastructure;

    public class Ef6DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Ef6DbSetWrapper(DbSet<TEntity> dbSet)
            : base(dbSet)
        {
            _dbSet = dbSet;
        }

        public override void Add(TEntity itemToAdd) => _dbSet.Add(itemToAdd);

        public override void Clear() => _dbSet.RemoveRange(_dbSet);
    }
}