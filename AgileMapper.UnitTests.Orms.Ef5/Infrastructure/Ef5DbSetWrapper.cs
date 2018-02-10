namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5.Infrastructure
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Orms.Infrastructure;

    public class Ef5DbSetWrapper<TEntity> : DbSetWrapperBase<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Ef5DbSetWrapper(DbContext context)
            : base(context.Set<TEntity>())
        {
            _dbSet = context.Set<TEntity>();
        }

        public override void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            => _dbSet.Include(navigationPropertyPath);

        public override Task Add(TEntity itemToAdd)
        {
            _dbSet.Add(itemToAdd);

            return Task.CompletedTask;
        }

        public override Task AddRange(TEntity[] itemsToAdd)
        {
            foreach (var entity in itemsToAdd)
            {
                _dbSet.Add(entity);
            }

            return Task.CompletedTask;
        }

        public override void Clear()
        {
            foreach (var entity in _dbSet.ToArray())
            {
                _dbSet.Remove(entity);
            }
        }
    }
}