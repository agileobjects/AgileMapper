namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringDataSources<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSources(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstant()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                await context.Products.Add(product);
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

        [Fact]
        public Task ShouldConditionallyApplyAConfiguredConstant()
        {
            return RunTest(async context =>
            {
                var product1 = new Product { Name = "P1" };
                var product2 = new Product { Name = "P2" };

                await context.Products.AddRange(product1, product2);
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
                        .OrderBy(p => p.ProductId)
                        .ToArray();

                    productDtos.Length.ShouldBe(2);

                    productDtos.First().ProductId.ShouldBe(product1.ProductId);
                    productDtos.First().Name.ShouldBe("P1");

                    productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                    productDtos.Second().Name.ShouldBe("PRODUCT!?");
                }
            });
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantToANestedMember()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Person 1",
                    Address = new Address { Line1 = "Line 1", Postcode = "Postcode" }
                };

                await context.Persons.Add(person);
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

                await context.Products.Add(product);
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

        protected Task DoShouldApplyMultipleConfiguredMembers()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "Product1" };

                await context.Products.Add(product);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<PersonDto>()
                        .Map(p => p.ProductId)
                        .To(dto => dto.Name)
                        .And
                        .Map(p => p.Name)
                        .To(p => p.Address.Line1);

                    var personDto = context
                        .Products
                        .ProjectUsing(mapper).To<PersonDto>()
                        .ShouldHaveSingleItem();

                    personDto.Id.ShouldBe(product.ProductId);
                    personDto.Name.ShouldBe(product.ProductId);
                    personDto.Address.ShouldNotBeNull();
                    personDto.Address.Line1.ShouldBe("Product1");
                }
            });
        }

        [Fact]
        public Task ShouldConditionallyApplyAConfiguredMember()
        {
            return RunTest(async context =>
            {
                var product1 = new Product { Name = "P.1" };
                var product2 = new Product { Name = "P.2" };

                await context.Products.AddRange(product1, product2);
                await context.SaveChanges();

                var product1Id = product1.ProductId;

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .If(p => p.ProductId > product1Id)
                        .Map(p => p.Name + "?!")
                        .To(dto => dto.Name);

                    var productDtos = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .OrderBy(p => p.ProductId)
                        .ToArray();

                    productDtos.Length.ShouldBe(2);

                    productDtos.First().ProductId.ShouldBe(product1.ProductId);
                    productDtos.First().Name.ShouldBe("P.1");

                    productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                    productDtos.Second().Name.ShouldBe("P.2?!");
                }
            });
        }

        [Fact]
        public Task ShouldApplyConditionalAndUnconditionalDataSourcesInOrder()
        {
            return RunTest(async context =>
            {
                var product1 = new Product { Name = "P.1" };
                var product2 = new Product { Name = "P.2" };

                await context.Products.AddRange(product1, product2);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Product>()
                        .ProjectedTo<ProductDto>()
                        .Map(p => p.Name + '!')
                        .To(dto => dto.Name)
                        .But
                        .If(p => p.Name.Contains("1"))
                        .Map(p => p.Name + '?')
                        .To(dto => dto.Name);

                    var productDtos = context
                        .Products
                        .ProjectUsing(mapper).To<ProductDto>()
                        .OrderBy(p => p.ProductId)
                        .ToArray();

                    productDtos.Length.ShouldBe(2);

                    productDtos.First().ProductId.ShouldBe(product1.ProductId);
                    productDtos.First().Name.ShouldBe("P.1?");

                    productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                    productDtos.Second().Name.ShouldBe("P.2!");
                }
            });
        }

        [Fact]
        public Task ShouldHandleANullMemberInACondition()
        {
            return RunTest(async context =>
            {
                var person1 = new Person { Name = "Frank", Address = new Address { Line1 = "Philly" } };
                var person2 = new Person { Name = "Dee" };

                await context.Persons.AddRange(person1, person2);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .ProjectedTo<PersonViewModel>()
                        .If(p => p.Address.Line2 == null)
                        .Map("None")
                        .To(dto => dto.AddressLine2);

                    var personDtos = context
                        .Persons
                        .ProjectUsing(mapper).To<PersonViewModel>()
                        .OrderBy(p => p.Id)
                        .ToArray();

                    personDtos.Length.ShouldBe(2);

                    personDtos.First().Id.ShouldBe(person1.PersonId);
                    personDtos.First().Name.ShouldBe("Frank");
                    personDtos.First().AddressId.ShouldBe(person1.Address.AddressId);
                    personDtos.First().AddressLine1.ShouldBe("Philly");
                    personDtos.First().AddressLine2.ShouldBe("None");
                    personDtos.First().AddressPostcode.ShouldBeNull();

                    personDtos.Second().Id.ShouldBe(person2.PersonId);
                    personDtos.Second().Name.ShouldBe("Dee");
                    personDtos.Second().AddressId.ShouldBeDefault();
                    personDtos.Second().AddressLine1.ShouldBeNull();
                    personDtos.Second().AddressLine2.ShouldBe("None");
                    personDtos.Second().AddressPostcode.ShouldBeNull();
                }
            });
        }

        [Fact]
        public Task ShouldSupportMultipleDivergedMappers()
        {
            return RunTest(async context =>
            {
                var address = new Address { Line1 = "Philly", Postcode = "PH1 1LY" };

                await context.Addresses.Add(address);
                await context.SaveChanges();

                using (var mapper1 = Mapper.CreateNew())
                using (var mapper2 = Mapper.CreateNew())
                {
                    mapper2.WhenMapping
                        .From<Address>()
                        .ProjectedTo<AddressDto>()
                        .Map(p => p.Line1)
                        .To(dto => dto.Line2);

                    var addressDto1 = context
                        .Addresses
                        .ProjectUsing(mapper1).To<AddressDto>()
                        .ShouldHaveSingleItem();

                    addressDto1.Id.ShouldBe(address.AddressId);
                    addressDto1.Line1.ShouldBe("Philly");
                    addressDto1.Line2.ShouldBeNull();
                    addressDto1.Postcode.ShouldBe("PH1 1LY");

                    var addressDto2 = context
                        .Addresses
                        .ProjectUsing(mapper2).To<AddressDto>()
                        .First();

                    addressDto2.Id.ShouldBe(address.AddressId);
                    addressDto2.Line1.ShouldBe("Philly");
                    addressDto2.Line2.ShouldBe("Philly");
                    addressDto2.Postcode.ShouldBe("PH1 1LY");
                }
            });
        }
    }
}