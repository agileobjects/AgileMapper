namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IDbSetWrapper<TEntity> : IQueryable<TEntity>
    {
        void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);

        void Add(TEntity itemToAdd);

        void Clear();
    }
}