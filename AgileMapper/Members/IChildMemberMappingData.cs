namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IChildMemberMappingData
    {
        MappingRuleSet RuleSet { get; }

        IObjectMappingData Parent { get; }

        IMemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }

    internal static class ChildMemberMappingDataExtensions
    {
        public static Expression GetRuleSetPopulationGuardOrNull(this IChildMemberMappingData childMappingData)
            => childMappingData.RuleSet.PopulationGuardFactory.GetPopulationGuardOrNull(childMappingData.MapperData);
    }
}