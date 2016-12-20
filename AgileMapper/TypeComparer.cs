namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif

    internal class TypeComparer : IComparer<Type>
    {
        public static readonly IComparer<Type> MostToLeastDerived = new TypeComparer();

        public int Compare(Type x, Type y)
        {
            if (x == y)
            {
                return 0;
            }

            if (y.IsAssignableFrom(x))
            {
                return -1;
            }

            return 1;
        }
    }
}