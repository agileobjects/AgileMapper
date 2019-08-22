namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class NullMemberPopulationGuardFactory
    {
        public static Expression Create(IMemberPopulationContext context)
            => context.PopulateCondition;
    }
}