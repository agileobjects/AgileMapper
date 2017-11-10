namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using System.Data.Entity;
    using System.Linq;
    using Orms.Infrastructure;

    public class Ef5DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Ef5DbSetWrapper(DbSet<TEntity> dbSet)
            : base(dbSet)
        {
            _dbSet = dbSet;
        }

        public override void Add(TEntity itemToAdd) => _dbSet.Add(itemToAdd);

        public override void Clear()
        {
            foreach (var entity in _dbSet.ToArray())
            {
                _dbSet.Remove(entity);
            }
        }
    }
}