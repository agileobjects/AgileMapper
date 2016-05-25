namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReadableExpressions.Extensions;

    internal class MemberName
    {
        private readonly string _name;
        private readonly List<string> _allNames;
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

            _allNames = new List<string> { name };

            var idNameParts = GetIdNameParts(declaringType).ToArray();
            IsIdentifier = idNameParts.Contains(name);

            if (IsIdentifier) { _allNames.AddRange(idNameParts); }

            if ((memberType == MemberType.GetMethod) ||
                (memberType == MemberType.SetMethod))
            {
                _allNames.Add(name.Substring(3));
            }

            JoiningName = (name == Constants.EnumerableElementMemberName) ? name : "." + name;
        }

        #region Setup

        private static IEnumerable<string> GetIdNameParts(Type declaringType)
        {
            yield return "Id";
            yield return "Identifier";

            if (declaringType == null)
            {
                yield break;
            }

            var declaringTypeName = declaringType.GetFriendlyName();

            yield return declaringTypeName + "Id";
            yield return declaringTypeName + "Identifier";
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