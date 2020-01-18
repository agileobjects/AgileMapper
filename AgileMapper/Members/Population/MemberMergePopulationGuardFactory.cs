namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MemberMergePopulationGuardFactory
    {
        public static Expression Create(IMemberPopulator populator)
        {
            var mapperData = populator.MapperData;
            var populateCondition = populator.PopulateCondition;

            if (SkipPopulationGuarding(mapperData))
            {
                return populateCondition;
            }

            var existingValueIsDefault = mapperData.TargetMember.GetHasDefaultValueCheck(mapperData);

            if (populateCondition == null)
            {
                return existingValueIsDefault;
            }

            return Expression.AndAlso(populateCondition, existingValueIsDefault);
        }

        private static bool SkipPopulationGuarding(IQualifiedMemberContext context)
        {
            var targetMember = context.TargetMember;

            if (!targetMember.IsReadable)
            {
                return true;
            }

            if (targetMember.IsSimple)
            {
                return false;
            }

            if (targetMember.Type != typeof(object))
            {
                return true;
            }

            var skipObjectValueGuarding = !targetMember.GuardObjectValuePopulations;

            return skipObjectValueGuarding;
        }
    }
}