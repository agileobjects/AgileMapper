namespace AgileObjects.AgileMapper.Caching
{
    using System.Collections.Generic;

    internal struct DefaultComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            // ReSharper disable once PossibleNullReferenceException
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        public int GetHashCode(T obj) => 0;
    }
}