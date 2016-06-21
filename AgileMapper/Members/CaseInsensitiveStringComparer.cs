namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public static IEqualityComparer<string> Instance = new CaseInsensitiveStringComparer();

        public static Expression InstanceMember =
            Expression.Field(null, typeof(CaseInsensitiveStringComparer), "Instance");

        public bool Equals(string x, string y) => x.Equals(y, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(string str) => 0; // <- to force use of Equals ^
    }
}