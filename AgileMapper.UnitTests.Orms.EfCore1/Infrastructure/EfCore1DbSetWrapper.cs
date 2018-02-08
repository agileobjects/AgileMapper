namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;

    public class EfCore1DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public EfCore1DbSetWrapper(DbContext context)
            : base(context.Set<TEntity>())
        {
            _dbSet = context.Set<TEntity>();
        }

        public override void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            => _dbSet.Include(navigationPropertyPath);

        public override void Add(TEntity itemToAdd) => _dbSet.Add(itemToAdd);

        public override void Clear() => _dbSet.RemoveRange(_dbSet);
    }
}