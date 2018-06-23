namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using NetStandardPolyfills;

    internal struct TypeComparer : IComparer<Type>
    {
        public static readonly IComparer<Type> MostToLeastDerived = default(TypeComparer);

        public int Compare(Type x, Type y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x.IsAssignableTo(y))
            {
                return -1;
            }

            return 1;
        }
    }
}