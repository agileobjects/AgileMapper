#if NET35
namespace AgileObjects.AgileMapper.Extensions.Internal
{
    internal static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
            => new Tuple<T1, T2>(item1, item2);
    }

    internal class Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; }

        public T2 Item2 { get; }
    }
}
#endif