namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal static class MemberPopulationFactory
    {
        public static IEnumerable<IMemberPopulation> Create(IObjectMappingContext omc)
        {
            return omc
                .GlobalContext
                .MemberFinder
                .GetTargetMembers(omc.ExistingObject.Type)
                .Select(targetMember => Create(targetMember, omc))
                .ToArray();
        }

        private static IMemberPopulation Create(Member targetMember, IObjectMappingContext omc)
        {
            var qualifiedMember = omc.TargetMember.Append(targetMember);
            var context = new MemberMappingContext(qualifiedMember, omc);

            Expression ignoreCondition;

            if (TargetMemberIsIgnored(context, out ignoreCondition) && (ignoreCondition == null))
            {
                return MemberPopulation.IgnoredMember(context);
            }

            var dataSource = omc.MapperContext.DataSources.FindFor(context);

            if (dataSource == null)
            {
                return MemberPopulation.NoDataSource(context);
            }

            var valueProviders = dataSource.GetValueProviders(context);
            var memberPopulation = new MemberPopulation(context, valueProviders).WithCondition(ignoreCondition);

            return memberPopulation;
        }

        private static bool TargetMemberIsIgnored(IMemberMappingContext context, out Expression ignoreCondition)
            => context.Parent.MapperContext.UserConfigurations.IsIgnored(context, out ignoreCondition);

        //var dataSourceValueFactory = ValueProvider.For(dataSource, context);
        //var valueFactories = new List<ValueProvider> { dataSourceValueFactory };

        //if (dataSourceCondition != null)
        //{
        //    var alternateDataSource = context.Parent
        //        .MapperContext
        //        .DataSources
        //        .FindFor(targetMember, DataSourceOption.ExcludeConfigured, context.Parent);

        //    if (alternateDataSource != null)
        //    {
        //        valueFactories.Add(ValueProvider.For(alternateDataSource, context));
        //    }
        //}
    }
}