namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using TestClasses;

    public interface ITestDbContext : IDisposable
    {
        IDbSetWrapper<Company> Companies { get; }

        IDbSetWrapper<Employee> Employees { get; }

        IDbSetWrapper<Category> Categories { get; }

        IDbSetWrapper<Product> Products { get; }

        IDbSetWrapper<Person> Persons { get; }

        IDbSetWrapper<Address> Addresses { get; }

        IDbSetWrapper<Rota> Rotas { get; }

        IDbSetWrapper<RotaEntry> RotaEntries { get; }

        IDbSetWrapper<Order> Orders { get; }

        IDbSetWrapper<OrderItem> OrderItems { get; }

        IDbSetWrapper<PublicBool> BoolItems { get; }

        IDbSetWrapper<PublicShort> ShortItems { get; }

        IDbSetWrapper<PublicInt> IntItems { get; }

        IDbSetWrapper<PublicLong> LongItems { get; }

        IDbSetWrapper<PublicString> StringItems { get; }

        void SaveChanges();
    }
}