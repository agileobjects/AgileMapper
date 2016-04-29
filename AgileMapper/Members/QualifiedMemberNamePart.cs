namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReadableExpressions.Extensions;

    internal class QualifiedMemberNamePart
    {
        private readonly List<string> _allNames;

        private QualifiedMemberNamePart(string name, Type declaringType, MemberType memberType)
        {
            _allNames = new List<string> { name };

            bool isRootNamePart;

            switch (name)
            {
                case "Source":
                    _allNames.Add("Target");
                    isRootNamePart = true;
                    break;

                case "Target":
                    _allNames.Add("Source");
                    isRootNamePart = true;
                    break;

                default:
                    isRootNamePart = false;
                    break;
            }

            var idNameParts = GetIdNameParts(declaringType).ToArray();
            IsIdentifier = idNameParts.Contains(name);

            if (IsIdentifier) { _allNames.AddRange(idNameParts); }

            if ((memberType == MemberType.GetMethod) ||
                (memberType == MemberType.SetMethod))
            {
                _allNames.Add(name.Substring(3));
            }

            JoiningName = (isRootNamePart || (name == Constants.EnumerableElementMemberName))
                ? name : "." + name;
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

        #region Factory Method

        public static QualifiedMemberNamePart For(Member member)
        {
            return new QualifiedMemberNamePart(member.Name, member.DeclaringType, member.MemberType);
        }

        #endregion

        public bool IsIdentifier { get; }

        public string JoiningName { get; }

        public IEnumerable<string> AllNames => _allNames;

        public bool Matches(QualifiedMemberNamePart otherNamePart)
        {
            return _allNames.Intersect(otherNamePart._allNames).Any();
        }
    }
}