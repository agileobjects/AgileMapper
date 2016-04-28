namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class DataSourceFinder
    {
        private readonly MemberFinder _memberFinder;

        public DataSourceFinder(MemberFinder memberFinder)
        {
            _memberFinder = memberFinder;
        }

        public IDataSource GetBestMatchFor(Member childTargetMember, IObjectMappingContext omc)
        {
            if (childTargetMember.IsComplex)
            {
                return new ComplexTypeMappingDataSource(childTargetMember);
            }

            var qualifiedTargetMember = omc.TargetMember.Append(childTargetMember);

            var bestMatchingSourceMember = GetSourceMemberMatching(qualifiedTargetMember, omc);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(omc.SourceObjectDepth);

            var sourceMemberDataSource = new SourceMemberDataSource(bestMatchingSourceMember);

            if (childTargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(sourceMemberDataSource, childTargetMember);
            }

            return sourceMemberDataSource;
        }

        private QualifiedMember GetSourceMemberMatching(
            QualifiedMember qualifiedTargetMember,
            IObjectMappingContext omc)
        {
            var rootSourceMember = Member
                .RootSource(omc.MappingContext.RootObjectMappingContext.SourceObject.Type);

            var qualifiedRootSourceMember = QualifiedMember.From(rootSourceMember);

            return GetAllSourceMembers(qualifiedRootSourceMember)
                .FirstOrDefault(sm => sm.Matches(qualifiedTargetMember));
        }

        private IEnumerable<QualifiedMember> GetAllSourceMembers(QualifiedMember parentMember)
        {
            yield return parentMember;

            foreach (var sourceMember in _memberFinder.GetSourceMembers(parentMember.Type))
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.Type.IsSimple())
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember))
                {
                    yield return qualifiedMember;
                }
            }
        }
    }
}