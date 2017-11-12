namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef6.Infrastructure
{
    using System;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using Orms.Infrastructure;

    public class Ef6DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Ef6DbSetWrapper(DbSet<TEntity> dbSet)
            : base(dbSet)
        {
            _dbSet = dbSet;
        }

        public override void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        {
            _dbSet.Include(navigationPropertyPath);
        }

        public override void Add(TEntity itemToAdd) => _dbSet.Add(itemToAdd);

        public override void Clear() => _dbSet.RemoveRange(_dbSet);
    }
}