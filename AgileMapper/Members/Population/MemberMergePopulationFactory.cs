namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberMergePopulationFactory : MemberPopulationFactoryBase
    {
        protected override Expression GetPopulationGuard(IMemberPopulationContext context)
        {
            var mapperData = context.MapperData;
            var populateCondition = context.PopulateCondition;

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

        private static bool SkipPopulationGuarding(IBasicMapperData mapperData)
        {
            var targetMember = mapperData.TargetMember;

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

        protected override Expression GetGuardedBindingValue(Expression bindingValue, Expression populationGuard)
            => bindingValue;
    }
}