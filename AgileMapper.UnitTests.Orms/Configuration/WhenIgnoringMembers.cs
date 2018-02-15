namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenIgnoringMembers<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenIgnoringMembers(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldIgnoreAConfiguredMember()
        {
            return RunTest(async (context, mapper) =>
            {
                var product = new Product { Name = "P1" };

                await context.Products.Add(product);
                await context.SaveChanges();

                mapper.WhenMapping
                    .From<Product>()
                    .ProjectedTo<ProductDto>()
                    .Ignore(p => p.Name);

                var productDto = context
                    .Products
                    .ProjectUsing(mapper).To<ProductDto>()
                    .ShouldHaveSingleItem();

                productDto.ProductId.ShouldBe(product.ProductId);
                productDto.Name.ShouldBeNull();
            });
        }

        [Fact]
        public Task ShouldIgnoreAConfiguredMemberConditionally()
        {
            return RunTest(async (context, mapper) =>
            {
                var product1 = new Product { Name = "P.1" };
                var product2 = new Product { Name = "P.2" };

                await context.Products.AddRange(product1, product2);
                await context.SaveChanges();

                mapper.WhenMapping
                    .From<Product>()
                    .ProjectedTo<ProductDto>()
                    .If(p => p.Name.EndsWith(2 + ""))
                    .Ignore(p => p.Name);

                var productDtos = context
                    .Products
                    .ProjectUsing(mapper).To<ProductDto>()
                    .OrderBy(p => p.ProductId)
                    .ToArray();

                productDtos.First().ProductId.ShouldBe(product1.ProductId);
                productDtos.First().Name.ShouldBe("P.1");
                productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                productDtos.Second().Name.ShouldBeNull();
            });
        }

        [Fact]
        public Task ShouldIgnoreMembersByTypeAndTargetType()
        {
            return RunTest(async (context, mapper) =>
            {
                var person = new Person
                {
                    Name = "Mac",
                    Address = new Address { Line1 = "1", Line2 = "2", Postcode = "3" }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                mapper.WhenMapping
                    .From<Address>()
                    .ProjectedTo<AddressDto>()
                    .IgnoreTargetMembersOfType<int>();

                var personDto = context
                    .Persons
                    .ProjectUsing(mapper).To<PersonDto>()
                    .ShouldHaveSingleItem();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Mac");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Id.ShouldNotBe(person.Address.AddressId);
                personDto.Address.Id.ShouldBeDefault();
                personDto.Address.Line1.ShouldBe("1");
                personDto.Address.Line2.ShouldBe("2");
                personDto.Address.Postcode.ShouldBe("3");
            });
        }

        [Fact]
        public Task ShouldIgnorePropertiesByPropertyInfoMatcher()
        {
            return RunTest(async (context, mapper) =>
            {
                var person = new Person
                {
                    Name = "Frankie",
                    Address = new Address { Line1 = "1", Line2 = "2", Postcode = "3" }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                mapper.WhenMapping
                    .To<AddressDto>()
                    .IgnoreTargetMembersWhere(member =>
                        member.IsPropertyMatching(p => p.GetGetMethod().Name.StartsWith("get_Line2")));

                mapper.WhenMapping
                    .From<Address>()
                    .ProjectedTo<AddressDto>()
                    .IgnoreTargetMembersWhere(member =>
                        member.IsPropertyMatching(p => p.GetSetMethod().Name.EndsWith("Line1")));

                var personDto = context
                    .Persons
                    .ProjectUsing(mapper).To<PersonDto>()
                    .ShouldHaveSingleItem();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Frankie");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Line1.ShouldBeNull();
                personDto.Address.Line2.ShouldBeNull();
                personDto.Address.Postcode.ShouldBe("3");
            });
        }
    }
}
