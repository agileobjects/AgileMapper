namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration.Inline
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringDataSourcesInline<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringDataSourcesInline(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantInline()
        {
            return RunTest(async context =>
            {
                var product = new Product { Name = "P1" };

                await context.Products.Add(product);
                await context.SaveChanges();

                var productDto = context
                    .Products
                    .Project().To<ProductDto>(cfg => cfg
                        .Map("PROD!!")
                        .To(dto => dto.Name))
                    .ShouldHaveSingleItem();

                productDto.ProductId.ShouldBe(product.ProductId);
                productDto.Name.ShouldBe("PROD!!");
            });
        }

        [Fact]
        public Task ShouldApplyAConfiguredConstantToANestedMemberInline()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Person One",
                    Address = new Address { Line1 = "Line One", Postcode = "Postcode" }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                var personDto = context
                    .Persons
                    .Project().To<PersonDto>(cfg => cfg
                        .WhenMapping
                        .ProjectionsTo<AddressDto>()
                        .Map("LINE TWO?!")
                        .To(a => a.Line2))
                    .ShouldHaveSingleItem();

                personDto.Id.ShouldBe(person.PersonId);
                personDto.Name.ShouldBe("Person One");
                personDto.Address.ShouldNotBeNull();
                personDto.Address.Line1.ShouldBe("Line One");
                personDto.Address.Line2.ShouldBe("LINE TWO?!");
                personDto.Address.Postcode.ShouldBe("Postcode");
            });
        }

        [Fact]
        public Task ShouldApplyAConfiguredExpressionUsingSourceMembersInline()
        {
            return RunTest(async context =>
            {
                var person1 = new Person
                {
                    Name = "Person One",
                    Address = new Address { Line1 = "One Street", Line2 = string.Empty, Postcode = "PSTCD" }
                };

                var person2 = new Person
                {
                    Name = "Person Two",
                    Address = new Address { Line1 = "Two Street", Line2 = "Two City", Postcode = "PSTCD" }
                };

                await context.Persons.AddRange(person1, person2);
                await context.SaveChanges();

                var personDtos = context
                    .Persons
                    .OrderBy(p => p.PersonId)
                    .Project().To<PersonDto>(cfg => cfg
                        .WhenMapping
                        .From<Address>()
                        .ProjectedTo<AddressDto>()
                        .If(a => string.Equals(a.Line1 + (object)a.Line2, a.Line1, StringComparison.OrdinalIgnoreCase))
                        .Map(a => a.Line1)
                        .To(a => a.Line2))
                    .ToArray();

                personDtos.Length.ShouldBe(2);

                personDtos[0].Id.ShouldBe(person1.PersonId);
                personDtos[0].Name.ShouldBe("Person One");
                personDtos[0].Address.ShouldNotBeNull();
                personDtos[0].Address.Line1.ShouldBe("One Street");
                personDtos[0].Address.Line2.ShouldBe("One Street");
                personDtos[0].Address.Postcode.ShouldBe("PSTCD");

                personDtos[1].Id.ShouldBe(person2.PersonId);
                personDtos[1].Name.ShouldBe("Person Two");
                personDtos[1].Address.ShouldNotBeNull();
                personDtos[1].Address.Line1.ShouldBe("Two Street");
                personDtos[1].Address.Line2.ShouldBe("Two City");
                personDtos[1].Address.Postcode.ShouldBe("PSTCD");
            });
        }
    }
}
