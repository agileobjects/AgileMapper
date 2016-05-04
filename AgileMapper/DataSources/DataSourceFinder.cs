namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;
    using ObjectPopulation;

    internal class DataSourceFinder
    {
        public IDataSource FindFor(QualifiedMember childTargetMember, IObjectMappingContext omc)
        {
            return FindFor(childTargetMember, omc, returnComplexTypeMapper: true);
        }

        public IDataSource FindBestMatchFor(QualifiedMember childTargetMember, IObjectMappingContext omc)
        {
            return FindFor(childTargetMember, omc, returnComplexTypeMapper: false);
        }

        private IDataSource FindFor(
            QualifiedMember qualifiedTargetMember,
            IObjectMappingContext omc,
            bool returnComplexTypeMapper)
        {
            IDataSource configuredDataSource;

            if (DataSourceIsConfigured(qualifiedTargetMember, omc, out configuredDataSource))
            {
                return GetFinalDataSource(qualifiedTargetMember, configuredDataSource, omc);
            }

            if (returnComplexTypeMapper && qualifiedTargetMember.IsComplex)
            {
                return new ComplexTypeMappingDataSource(qualifiedTargetMember.LeafMember, omc);
            }

            return GetSourceMemberDataSourceOrNull(qualifiedTargetMember, omc);
        }

        private static bool DataSourceIsConfigured(
            QualifiedMember targetMember,
            IObjectMappingContext omc,
            out IDataSource configuredDataSource)
        {
            var memberContext = new MemberMappingContext(targetMember, omc);

            configuredDataSource = omc
                .MapperContext
                .UserConfigurations
                .GetDataSourceOrNull(memberContext);

            return configuredDataSource != null;
        }

        private IDataSource GetSourceMemberDataSourceOrNull(
            QualifiedMember qualifiedTargetMember,
            IObjectMappingContext omc)
        {
            var bestMatchingSourceMember = GetSourceMemberMatching(qualifiedTargetMember, omc);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(omc.SourceObjectDepth);

            var sourceMemberDataSource = new SourceMemberDataSource(bestMatchingSourceMember, omc);

            return GetFinalDataSource(qualifiedTargetMember, sourceMemberDataSource, omc);
        }

        private static IDataSource GetFinalDataSource(
            QualifiedMember qualifiedTargetMember,
            IDataSource foundDataSource,
            IObjectMappingContext omc)
        {
            if (qualifiedTargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(
                    foundDataSource,
                    qualifiedTargetMember.LeafMember,
                    omc);
            }

            return foundDataSource;
        }

        public QualifiedMember GetSourceMemberMatching(QualifiedMember qualifiedTargetMember, IObjectMappingContext omc)
        {
            var rootSourceMember = omc.SourceMember;

            return GetAllSourceMembers(rootSourceMember, omc)
                .FirstOrDefault(sm => sm.Matches(qualifiedTargetMember));
        }

        private static IEnumerable<QualifiedMember> GetAllSourceMembers(
            QualifiedMember parentMember,
            IObjectMappingContext currentOmc)
        {
            yield return parentMember;

            var parentMemberType = currentOmc.GetSourceMemberRuntimeType(parentMember);

            foreach (var sourceMember in currentOmc.GlobalContext.MemberFinder.GetSourceMembers(parentMemberType))
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, currentOmc))
                {
                    yield return qualifiedMember;
                }
            }
        }
    }
}