namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Queryables.Api;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<EfCore2TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        protected override Type GetQueryProviderType(QueryProviderTypeSelector selector)
            => selector.Using<EntityQueryProvider>();
    }
}
