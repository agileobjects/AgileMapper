namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal class QualifiedMemberName
    {
        private readonly MemberName[] _nameParts;
        private readonly IEnumerable<string> _allJoinedNames;

        public QualifiedMemberName(MemberName[] nameParts)
        {
            Joined = GetJoinedName(nameParts);

            _nameParts = new MemberName[nameParts.Length - 1]; // <- Don't bother with the root property
            Array.Copy(nameParts, 1, _nameParts, 0, _nameParts.Length);

            _allJoinedNames = _nameParts
                .Select(np => np.AllNames)
                .CartesianProduct()
                .Select(p => string.Join(string.Empty, p))
                .ToArray();
        }

        private static string GetJoinedName(IEnumerable<MemberName> nameParts)
        {
            return string.Join(string.Empty, nameParts.Select(np => np.JoiningName));
        }

        public string Joined { get; }

        public bool Matches(QualifiedMemberName otherQualifiedName)
        {
            if (_allJoinedNames.Intersect(otherQualifiedName._allJoinedNames).Any())
            {
                return true;
            }

            if (_nameParts.Length != otherQualifiedName._nameParts.Length)
            {
                return false;
            }

            return !_nameParts
                .Where((t, i) => !t.Matches(otherQualifiedName._nameParts[i]))
                .Any();
        }
    }
}