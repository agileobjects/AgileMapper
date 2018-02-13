namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenConfiguringObjectCreation<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConfiguringObjectCreation(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldUseACustomObjectFactory()
        {
            return RunTest(async context =>
            {
                await context.StringItems.Add(new PublicString { Value = "Ctor!" });
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .ProjectionsTo<PublicStringDto>()
                        .CreateInstancesUsing(o => new PublicStringDto { Value = "PANTS" });

                    var ctorDto = context
                        .StringItems
                        .ProjectUsing(mapper)
                        .To<PublicStringDto>()
                        .ShouldHaveSingleItem();

                    ctorDto.Value.ShouldBe("PANTS");
                }
            });
        }

        [Fact]
        public Task ShouldUseACustomObjectFactoryForASpecifiedType()
        {
            return RunTest(async context =>
            {
                var person = new Person
                {
                    Name = "Fatima",
                    Address = new Address { Line1 = "1", Line2 = "2" }
                };

                await context.Persons.Add(person);
                await context.SaveChanges();

                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Person>()
                        .ProjectedTo<PersonDto>()
                        .CreateInstancesOf<AddressDto>().Using(p => new AddressDto
                        {
                            Line1 = p.Address.Line1 + "!",
                            Line2 = p.Address.Line2 + "?",
                            Postcode = "BA7 8RD"
                        });

                    var personDto = context
                        .Persons
                        .ProjectUsing(mapper)
                        .To<PersonDto>()
                        .ShouldHaveSingleItem();

                    personDto.Address.ShouldNotBeNull();
                    personDto.Address.Line1.ShouldBe("1!");
                    personDto.Address.Line2.ShouldBe("2?");
                    personDto.Address.Postcode.ShouldBe("BA7 8RD");
                }
            });
        }

        #region Project -> Conditional Factory

        protected Task RunShouldUseAConditionalObjectFactory()
            => RunTest(DoShouldUseAConditionalObjectFactory);

        protected Task RunShouldErrorUsingAConditionalObjectFactory()
            => RunTestAndExpectThrow(DoShouldUseAConditionalObjectFactory);

        private static async Task DoShouldUseAConditionalObjectFactory(TOrmContext context)
        {
            await context.IntItems.Add(new PublicInt { Value = 1 });
            await context.IntItems.Add(new PublicInt { Value = 2 });
            await context.IntItems.Add(new PublicInt { Value = 3 });
            await context.SaveChanges();

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .ProjectedTo<PublicStringCtorDto>()
                    .If(p => p.Value % 2 == 0)
                    .CreateInstancesUsing(p => new PublicStringCtorDto((p.Value * 2).ToString()));

                var stringDtos = context
                    .IntItems
                    .OrderBy(p => p.Id)
                    .ProjectUsing(mapper)
                    .To<PublicStringCtorDto>()
                    .ToArray();

                stringDtos.First().Value.ShouldBe("1");
                stringDtos.Second().Value.ShouldBe("4");
                stringDtos.Third().Value.ShouldBe("3");
            }
        }

        #endregion
    }
}
