namespace AgileObjects.AgileMapper.UnitTests.Orms.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using TestClasses;

    public interface ITestDbContext : IDisposable
    {
        IDbSetWrapper<Animal> Animals { get; }

        IDbSetWrapper<Shape> Shapes { get; }

        IDbSetWrapper<Company> Companies { get; }

        IDbSetWrapper<Employee> Employees { get; }

        IDbSetWrapper<Category> Categories { get; }

        IDbSetWrapper<Product> Products { get; }

        IDbSetWrapper<Person> Persons { get; }

        IDbSetWrapper<Address> Addresses { get; }

        IDbSetWrapper<Rota> Rotas { get; }

        IDbSetWrapper<RotaEntry> RotaEntries { get; }

        IDbSetWrapper<OrderUk> Orders { get; }

        IDbSetWrapper<OrderItem> OrderItems { get; }

        IDbSetWrapper<PublicBool> BoolItems { get; }

        IDbSetWrapper<PublicByte> ByteItems { get; }

        IDbSetWrapper<PublicShort> ShortItems { get; }

        IDbSetWrapper<PublicInt> IntItems { get; }

        IDbSetWrapper<PublicLong> LongItems { get; }

        IDbSetWrapper<PublicDecimal> DecimalItems { get; }

        IDbSetWrapper<PublicDouble> DoubleItems { get; }

        IDbSetWrapper<PublicDateTime> DateTimeItems { get; }

        IDbSetWrapper<PublicString> StringItems { get; }

        IDbSetWrapper<PublicTitle> TitleItems { get; }

        Task SaveChanges();
    }
}