#if NET35
namespace AgileObjects.AgileMapper.Extensions.Internal
{
    internal static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
            => new Tuple<T1, T2>(item1, item2);
    }

    /// <summary>
    /// Tuple{T1, T2} polyfill for .NET 3.5
    /// </summary>
    /// <typeparam name="T1">The first type of object stored in the Tuple{T1, T2}.</typeparam>
    /// <typeparam name="T2">The second type of object stored in the Tuple{T1, T2}.</typeparam>
    public class Tuple<T1, T2>
    {
        /// <summary>
        /// Initializes a new instance of the Tuple{T1, T2} class.
        /// </summary>
        /// <param name="item1">The first item to store in the Tuple{T1, T2}.</param>
        /// <param name="item2">The second item to store in the Tuple{T1, T2}.</param>
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Gets the first item stored in the Tuple{T1, T2}.
        /// </summary>
        public T1 Item1 { get; }

        /// <summary>
        /// Gets the second item stored in the Tuple{T1, T2}.
        /// </summary>
        public T2 Item2 { get; }
    }
}
#endif