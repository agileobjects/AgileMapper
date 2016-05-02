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
                return configuredDataSource;
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
            var configurationContext = new ConfigurationContext(targetMember, omc);

            configuredDataSource = omc
                .MapperContext
                .UserConfigurations
                .GetDataSourceOrNull(configurationContext);

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

            if (qualifiedTargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(
                    sourceMemberDataSource,
                    qualifiedTargetMember.LeafMember,
                    omc);
            }

            return sourceMemberDataSource;
        }

        public QualifiedMember GetSourceMemberMatching(QualifiedMember qualifiedTargetMember, IObjectMappingContext omc)
        {
            var rootSourceMember = omc.SourceMember;
            var memberFinder = omc.GlobalContext.MemberFinder;

            return GetAllSourceMembers(rootSourceMember, memberFinder)
                .FirstOrDefault(sm => sm.Matches(qualifiedTargetMember));
        }

        private static IEnumerable<QualifiedMember> GetAllSourceMembers(
            QualifiedMember parentMember,
            MemberFinder memberFinder)
        {
            yield return parentMember;

            foreach (var sourceMember in memberFinder.GetSourceMembers(parentMember.Type))
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, memberFinder))
                {
                    yield return qualifiedMember;
                }
            }
        }
    }
}