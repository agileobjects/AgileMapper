namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions;

    internal static class ConfigInfoDataSourceExtensions
    {
        public static ISequencedDataSourceFactory[] GetSequenceDataSourceFactories(
            this MappingConfigInfo configInfo)
        {
            return configInfo.Get<ISequencedDataSourceFactory[]>();
        }

        public static MappingConfigInfo ForSequentialConfiguration(
            this MappingConfigInfo configInfo,
            ISequencedDataSourceFactory[] dataSourceFactorySequence)
        {
            return configInfo.Copy()
                .ForSequentialConfiguration()
                .SetSequenceDataSourceFactories(dataSourceFactorySequence);
        }

        public static MappingConfigInfo SetSequenceDataSourceFactories(
            this MappingConfigInfo configInfo,
            ISequencedDataSourceFactory[] dataSourceFactorySequence)
        {
            return configInfo.Set(dataSourceFactorySequence);
        }

        public static bool HasTargetMemberFilter(this MappingConfigInfo configInfo)
            => configInfo.HasTargetMemberFilter(out _);

        public static bool HasTargetMemberFilter(
            this MappingConfigInfo configInfo,
            out Expression<Func<TargetMemberSelector, bool>> targetMemberFilter)
        {
            targetMemberFilter = configInfo.Get<Expression<Func<TargetMemberSelector, bool>>>();
            return targetMemberFilter != null;
        }

        public static MappingConfigInfo SetTargetMemberFilter(
            this MappingConfigInfo configInfo,
            Expression<Func<TargetMemberSelector, bool>> memberMatcherLambda)
        {
            return configInfo.Set(memberMatcherLambda);
        }

        public static void ThrowIfTargetMemberFilterSpecified<TTarget, TTargetValue>(
            this MappingConfigInfo configInfo,
            Func<MappingConfigInfo, string> configDescriptionFactory,
            params Expression<Func<TTarget, TTargetValue>>[] targetMembers)
        {
            if (!configInfo.HasTargetMemberFilter(out var filter))
            {
                return;
            }

            var configDescription = configDescriptionFactory.Invoke(configInfo);

            var targetMemberPaths = targetMembers
                .Select(m => m
                    .ToTargetMemberOrNull(configInfo.MapperContext)
                    .GetFriendlyTargetPath(configInfo))
                .Join(", ");

            throw new MappingConfigurationException(
                $"Member-agnostic target member filter '{filter.Body.ToReadableString()}' cannot " +
                $"be combined with member-specific {configDescription} {targetMemberPaths}");
        }
    }
}