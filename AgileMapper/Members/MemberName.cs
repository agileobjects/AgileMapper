namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReadableExpressions.Extensions;

    internal class MemberName
    {
        private readonly string _name;
        private readonly ICollection<string> _allNames;
        private readonly bool _isRoot;

        public MemberName(
            string name,
            Type declaringType,
            MemberType memberType,
            bool isRoot)
        {
            _name = name;
            _isRoot = isRoot;

            if (isRoot)
            {
                JoiningName = name;
                return;
            }

            var idNameParts = GetIdNameParts(declaringType);
            IsIdentifier = idNameParts.Contains(name);

            _allNames = IsIdentifier ? idNameParts : new List<string> { name };

            switch (memberType)
            {
                case MemberType.GetMethod:
                case MemberType.SetMethod:
                    _allNames.Add(name.Substring(3));
                    break;
            }

            JoiningName = (name == Constants.EnumerableElementMemberName) ? name : "." + name;
        }

        #region Setup

        private static ICollection<string> GetIdNameParts(Type declaringType)
        {
            var declaringTypeName = declaringType.GetFriendlyName();

            return new List<string> { "Id", "Identifier", declaringTypeName + "Id", declaringTypeName + "Identifier" };
        }

        #endregion

        public bool IsIdentifier { get; }

        public string JoiningName { get; }

        public IEnumerable<string> AllNames => _allNames;

        public bool Matches(MemberName otherName)
        {
            return (_isRoot && otherName._isRoot) ||
                _name.Equals(otherName._name, StringComparison.OrdinalIgnoreCase) ||
                _allNames.Intersect(otherName._allNames, CaseInsensitiveStringComparer.Instance).Any();
        }
    }
}