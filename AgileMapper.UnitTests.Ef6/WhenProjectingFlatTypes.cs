﻿namespace AgileObjects.AgileMapper.UnitTests.Ef6
{
    using Infrastructure;
    using Orms;
    using Orms.Infrastructure;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<Ef6TestDbContext>
    {
        public WhenProjectingFlatTypes(TestContext context)
            : base(context)
        {
        }
    }
}