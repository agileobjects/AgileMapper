namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal class QualifiedMemberName : IQualifiedMemberName
    {
        private readonly MemberName[] _nameParts;
        private readonly IEnumerable<string> _allJoinedNames;

        public QualifiedMemberName(MemberName[] nameParts)
        {
            _nameParts = new MemberName[nameParts.Length - 1]; // <- Don't bother with the root property
            Array.Copy(nameParts, 1, _nameParts, 0, _nameParts.Length);

            _allJoinedNames = _nameParts
                .Select(np => np.AllNames)
                .CartesianProduct()
                .Select(p => string.Join(string.Empty, p))
                .ToArray();
        }


        public bool Matches(IQualifiedMemberName otherQualifiedName)
        {
            var otherName = otherQualifiedName as QualifiedMemberName;

            if (otherName == null)
            {
                return otherQualifiedName.Matches(this);
            }

            if (_allJoinedNames.Intersect(otherName._allJoinedNames, CaseInsensitiveStringComparer.Instance).Any())
            {
                return true;
            }

            if (_nameParts.Length != otherName._nameParts.Length)
            {
                return false;
            }

            return _nameParts
                .Where((t, i) => !t.Matches(otherName._nameParts[i]))
                .None();
        }
    }
}