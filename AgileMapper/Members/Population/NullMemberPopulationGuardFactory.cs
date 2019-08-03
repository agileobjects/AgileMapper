namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal partial struct NullMemberPopulationGuardFactory : IPopulationGuardFactory
    {
        public Expression GetPopulationGuard(IMemberPopulationContext context)
            => context.PopulateCondition;
    }
}