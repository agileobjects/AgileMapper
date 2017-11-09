﻿namespace AgileObjects.AgileMapper.UnitTests.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToBools : WhenConvertingToBools<Ef6TestDbContext>
    {
        public WhenConvertingToBools(TestContext context)
            : base(context)
        {
        }
    }
}
