namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConfiguringDataSources<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSources(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        protected Task DoShouldApplyAConfiguredConstant()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                context.Products.Add(product);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .Map("PRODUCT")
                        .To(dto => dto.Name);

                    var productDto = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .ShouldHaveSingleItem();

                    productDto.ProductId.ShouldBe(product.ProductId);
                    productDto.Name.ShouldBe("PRODUCT");
                }
            });
        }

        protected Task DoShouldConditionallyApplyAConfiguredConstant()
        {
            return RunTest(async context =>
            {
                var product1 = new Product { Name = "P1" };
                var product2 = new Product { Name = "P2" };

                context.Products.Add(product1);
                context.Products.Add(product2);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .If(p => p.Name == "P2")
                        .Map("PRODUCT!?")
                        .To(dto => dto.Name);

                    var productDtos = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .ToArray();

                    productDtos.Length.ShouldBe(2);

                    productDtos.First().ProductId.ShouldBe(product1.ProductId);
                    productDtos.First().Name.ShouldBe("P1");

                    productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                    productDtos.Second().Name.ShouldBe("PRODUCT!?");
                }
            });
        }

        protected Task DoShouldApplyAConfiguredConstantToANestedMember()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Person 1",
                    Address = new Address { Line1 = "Line 1", Postcode = "Postcode" }
                };

                context.Persons.Add(person);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .ProjectedTo<PersonDto>()
                        .Map("LINE ONE!?")
                        .To(dto => dto.Address.Line1);

                    var personDto = context
                        .Persons
                        .ProjectUsing(mapper).To<PersonDto>()
                        .ShouldHaveSingleItem();

                    personDto.Id.ShouldBe(person.PersonId);
                    personDto.Name.ShouldBe("Person 1");
                    personDto.Address.ShouldNotBeNull();
                    personDto.Address.Line1.ShouldBe("LINE ONE!?");
                    personDto.Address.Postcode.ShouldBe("Postcode");
                }
            });
        }

        protected Task DoShouldApplyAConfiguredMember()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                context.Products.Add(product);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<PersonDto>()
                        .Map(p => p.ProductId)
                        .To(dto => dto.Name);

                    var personDto = context
                        .Products
                        .ProjectUsing(mapper).To<PersonDto>()
                        .ShouldHaveSingleItem();

                    personDto.Id.ShouldBe(product.ProductId);
                    personDto.Name.ShouldBe(product.ProductId);
                }
            });
        }
    }
}