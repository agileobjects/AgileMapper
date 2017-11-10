namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using TestClasses;

    public interface ITestDbContext : IDisposable
    {
        bool StringParsingSupported { get; }

        IDbSetWrapper<Product> Products { get; }

        IDbSetWrapper<PublicBoolProperty> BoolItems { get; }

        IDbSetWrapper<PublicShortProperty> ShortItems { get; }

        IDbSetWrapper<PublicIntProperty> IntItems { get; }

        IDbSetWrapper<PublicLongProperty> LongItems { get; }

        IDbSetWrapper<PublicStringProperty> StringItems { get; }

        void SaveChanges();
    }
}