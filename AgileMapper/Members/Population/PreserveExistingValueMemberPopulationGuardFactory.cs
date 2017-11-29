namespace AgileObjects.AgileMapper.Members.Population
{
    using System.Linq.Expressions;
    using Members;

    internal class PreserveExistingValueMemberPopulationGuardFactory : IMemberPopulationGuardFactory
    {
        public Expression GetPopulationGuardOrNull(IMemberMapperData mapperData)
        {
            if (SkipPopulateCondition(mapperData))
            {
                return null;
            }

            var existingValueIsDefault = mapperData.TargetMember.GetHasDefaultValueCheck(mapperData);

            return existingValueIsDefault;
        }

        private static bool SkipPopulateCondition(IBasicMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsReadable)
            {
                return true;
            }

            if (mapperData.TargetMember.IsSimple)
            {
                return false;
            }

            if (mapperData.TargetMember.Type != typeof(object))
            {
                return true;
            }

            var skipObjectValueGuarding = !mapperData.TargetMember.GuardObjectValuePopulations;

            return skipObjectValueGuarding;
        }
    }
}