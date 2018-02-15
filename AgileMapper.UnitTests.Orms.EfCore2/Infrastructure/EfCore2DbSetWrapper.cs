namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;

    public class EfCore2DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public EfCore2DbSetWrapper(DbContext context)
            : base(context.Set<TEntity>())
        {
            _dbSet = context.Set<TEntity>();
        }

        public override void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            => _dbSet.Include(navigationPropertyPath);

        public override Task Add(TEntity itemToAdd) => _dbSet.AddAsync(itemToAdd);

        public override Task AddRange(TEntity[] itemsToAdd) => _dbSet.AddRangeAsync(itemsToAdd);

        public override void Clear() => _dbSet.RemoveRange(_dbSet);
    }
}