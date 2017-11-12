namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public abstract class DbSetWrapperBase<TEntity> : IDbSetWrapper<TEntity>
    {
        private readonly IQueryable<TEntity> _dbSet;

        protected DbSetWrapperBase(IQueryable<TEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        public IEnumerator<TEntity> GetEnumerator() => _dbSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Expression Expression => _dbSet.Expression;

        public Type ElementType => _dbSet.ElementType;

        public IQueryProvider Provider => _dbSet.Provider;

        public abstract void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);

        public abstract void Add(TEntity itemToAdd);

        public abstract void Clear();
    }
}