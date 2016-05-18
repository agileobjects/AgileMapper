namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;

    internal class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public static IEqualityComparer<string> Instance = new CaseInsensitiveStringComparer();

        public bool Equals(string x, string y) => x.Equals(y, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(string str) => str.GetHashCode();
    }
}