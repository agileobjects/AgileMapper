namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDbSetWrapper<TEntity> : IQueryable<TEntity>
    {
        void Include<TProperty>(Expression<Func<TEntity, TProperty>> navigationPropertyPath);

        Task Add(TEntity itemToAdd);

        Task AddRange(params TEntity[] itemsToAdd);

        void Clear();
    }
}