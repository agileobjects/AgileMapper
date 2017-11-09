﻿namespace AgileObjects.AgileMapper.UnitTests.Ef6.SimpleTypeConversion
{
    using Infrastructure;
    using Orms.Infrastructure;
    using Orms.SimpleTypeConversion;

    public class WhenConvertingToInts : WhenConvertingToInts<Ef6TestDbContext>
    {
        public WhenConvertingToInts(TestContext context)
            : base(context)
        {
        }
    }
}