namespace AgileObjects.AgileMapper.Caching
{
    using System.Collections.Generic;

    internal struct ReferenceEqualsComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}