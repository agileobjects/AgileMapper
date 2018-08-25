﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.Configuration
{
    using System.Linq;
    using System.Threading.Tasks;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenMappingToNull<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenMappingToNull(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Project -> Configured Null

        protected Task RunShouldApplyAUserConfiguration()
            => RunTest(DoShouldApplyAUserConfiguration);

        protected Task RunShouldErrorApplyingAUserConfiguration()
            => RunTestAndExpectThrow(DoShouldApplyAUserConfiguration);

        private static async Task DoShouldApplyAUserConfiguration(TOrmContext context, IMapper mapper)
        {
            var person1 = new Person
            {
                Name = "Frank",
                Address = new Address { Line1 = "1" }
            };

            var person2 = new Person
            {
                Name = "Dee",
                Address = new Address { Line1 = "Paddie's Pub" }
            };

            await context.Persons.AddRange(person1, person2);
            await context.SaveChanges();

            mapper.WhenMapping
                .From<Address>()
                .ProjectedTo<AddressDto>()
                .If(a => a.Line1.Length == 1)
                .MapToNull();

            var personDtos = context
                .Persons
                .ProjectUsing(mapper)
                .To<PersonDto>()
                .OrderBy(p => p.Id)
                .ToArray();

            personDtos.Length.ShouldBe(2);

            personDtos.First().Name.ShouldBe("Frank");
            personDtos.First().Address.ShouldBeNull();

            personDtos.Second().Name.ShouldBe("Dee");
            personDtos.Second().Address.ShouldNotBeNull();
            personDtos.Second().Address.Line1.ShouldBe("Paddie's Pub");
        }

        #endregion
    }
}
