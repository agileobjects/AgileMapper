namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System.Linq;

    public interface IDbSetWrapper<TEntity> : IQueryable<TEntity>
    {
        void Add(TEntity itemToAdd);

        void Clear();
    }
}