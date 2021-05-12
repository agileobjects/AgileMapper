namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using ReadableExpressions;

    internal class NullMemberPopulator : IMemberPopulator
    {
        private readonly Expression _description;

        public NullMemberPopulator(Expression description, IMemberMapperData mapperData)
        {
            _description = description;
            MapperData = mapperData;
        }

        #region Factory Methods

        public static IMemberPopulator Unmappable(MemberPopulationContext context, string reason)
            => CreateNullMemberPopulator(context, targetMember => $"No way to populate {targetMember.Name} ({reason})");

        public static IMemberPopulator IgnoredMember(MemberPopulationContext context)
            => CreateNullMemberPopulator(context, context.MemberIgnore.GetIgnoreMessage);

        public static IMemberPopulator NoDataSources(MemberPopulationContext context)
        {
            var noDataSourcesMessage = CreateNullPopulatorDescription(
                GetNoDataSourcesMessage,
                context.MemberMapperData);

            var noDataSource = new NullDataSource(noDataSourcesMessage);
            var noDataSources = DataSourceSet.For(noDataSource, context);

            context.MemberMapperData.RegisterTargetMemberDataSources(noDataSources);

            return CreateNullMemberPopulator(noDataSourcesMessage, context, (msg, _) => msg);
        }

        private static string GetNoDataSourcesMessage(QualifiedMember targetMember)
        {
            return targetMember.IsSimple
                ? "No data sources for " + targetMember.Name
                : $"No data sources for {targetMember.Name} or any of its child members";
        }

        private static IMemberPopulator CreateNullMemberPopulator(
            MemberPopulationContext context,
            Func<QualifiedMember, string> commentFactory)
        {
            return CreateNullMemberPopulator(
                commentFactory,
                context,
                CreateNullPopulatorDescription);
        }

        private static IMemberPopulator CreateNullMemberPopulator<TArg>(
            TArg argument,
            MemberPopulationContext context,
            Func<TArg, IMemberMapperData, Expression> descriptionFactory)
        {
            if (!context.MappingContext.PlanSettings.CommentUnmappedMembers)
            {
                return null;
            }

            var mapperData = context.MemberMapperData;
            var description = descriptionFactory.Invoke(argument, mapperData);

            return new NullMemberPopulator(description, mapperData);
        }

        private static Expression CreateNullPopulatorDescription(
            Func<QualifiedMember, string> commentFactory,
            IQualifiedMemberContext context)
        {
            return ReadableExpression.Comment(commentFactory.Invoke(context.TargetMember));
        }

        #endregion

        public IMemberMapperData MapperData { get; }

        public bool CanPopulate => false;

        public Expression PopulateCondition => null;

        public Expression GetPopulation() => _description;
    }
}