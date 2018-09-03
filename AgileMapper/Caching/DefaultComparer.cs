namespace AgileObjects.AgileMapper.Caching
{
    internal struct DefaultComparer<T> : IKeyComparer<T>
    {
        public bool UseHashCodes => false;

        public bool Equals(T x, T y)
        {
            // ReSharper disable once PossibleNullReferenceException
            return ReferenceEquals(x, y) || x.Equals(y);
        }
    }
}