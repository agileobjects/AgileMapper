namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;
    using ObjectPopulation;

    internal class DataSourceFinder
    {
        public IDataSource FindFor(IMemberMappingContext context) => FindFor(context, DataSourceOption.None);

        public IDataSource FindFor(IMemberMappingContext context, DataSourceOption options)
        {
            if (context.Parent == null)
            {
                return GetSourceMemberDataSourceFor(
                    QualifiedMember.From(Member.RootSource(context.SourceObject.Type)),
                    context);
            }

            IDataSource configuredDataSource;

            if (!options.HasFlag(DataSourceOption.ExcludeConfigured) &&
                DataSourceIsConfigured(context, out configuredDataSource))
            {
                return configuredDataSource;
            }

            if (!options.HasFlag(DataSourceOption.ExcludeComplexTypeMapping) &&
                context.TargetMember.IsComplex)
            {
                return new ComplexTypeMappingDataSource(context);
            }

            return GetSourceMemberDataSourceOrNull(context);
        }

        private static bool DataSourceIsConfigured(IMemberMappingContext context, out IDataSource configuredDataSource)
        {
            configuredDataSource = (context.Parent ?? (IObjectMappingContext)context)
                .MapperContext
                .UserConfigurations
                .GetDataSourceOrNull(context);

            if (configuredDataSource == null)
            {
                return false;
            }

            configuredDataSource = GetFinalDataSource(configuredDataSource, context);
            return true;
        }

        private IDataSource GetSourceMemberDataSourceOrNull(IMemberMappingContext context)
        {
            var bestMatchingSourceMember = GetSourceMemberMatching(context);

            if (bestMatchingSourceMember == null)
            {
                return null;
            }

            bestMatchingSourceMember = bestMatchingSourceMember.RelativeTo(context.SourceObjectDepth);

            return GetSourceMemberDataSourceFor(bestMatchingSourceMember, context);
        }

        private static IDataSource GetSourceMemberDataSourceFor(QualifiedMember sourceMember, IMemberMappingContext context)
        {
            var sourceMemberDataSource = new SourceMemberDataSource(sourceMember, context);

            return GetFinalDataSource(sourceMemberDataSource, context);
        }

        private static IDataSource GetFinalDataSource(IDataSource foundDataSource, IMemberMappingContext context)
            => context.TargetMember.IsEnumerable ? new EnumerableMappingDataSource(foundDataSource, context) : foundDataSource;

        public QualifiedMember GetSourceMemberMatching(IMemberMappingContext context)
        {
            var rootSourceMember = context.Parent.SourceMember;

            return GetAllSourceMembers(rootSourceMember, context.Parent)
                .FirstOrDefault(sm => sm.Matches(context.TargetMember));
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