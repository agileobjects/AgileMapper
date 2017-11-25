namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore1
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Queryables.Api;

    public class WhenViewingMappingPlans : WhenViewingMappingPlans<EfCore1TestDbContext>
    {
        public WhenViewingMappingPlans(InMemoryEfCore1TestContext context)
            : base(context)
        {
        }

        protected override Type GetQueryProviderType(QueryProviderTypeSelector selector)
            => selector.Using<EntityQueryProvider>();
    }
}
