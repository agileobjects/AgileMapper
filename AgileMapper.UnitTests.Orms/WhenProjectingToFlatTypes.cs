namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Linq;
    using Infrastructure;
    using TestClasses;
    using Xunit;

    public abstract class WhenProjectingToFlatTypes<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenProjectingToFlatTypes(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldProjectAComplexTypeMemberToAFlatTypeList()
        {
            RunTest(context =>
            {
                var person1 = new Person
                {
                    Name = "Person One",
                    Address = new Address
                    {
                        Line1 = "Person One Address Line 1",
                        Line2 = "Person One Address Line 2",
                        Postcode = "Person One Address Postcode"
                    }
                };

                var person2 = new Person { Name = "Person Two" };

                context.Persons.Add(person1);
                context.Persons.Add(person2);
                context.SaveChanges();

                var personViewModels = context
                    .Persons
                    .Project().To<PersonViewModel>()
                    .OrderBy(pvm => pvm.Id)
                    .ToList();

                personViewModels.Count.ShouldBe(2);

                personViewModels[0].Id.ShouldBe(person1.PersonId);
                personViewModels[0].Name.ShouldBe("Person One");
                personViewModels[0].AddressId.ShouldBe(person1.Address.AddressId);
                personViewModels[0].AddressLine1.ShouldBe("Person One Address Line 1");
                personViewModels[0].AddressLine2.ShouldBe("Person One Address Line 2");
                personViewModels[0].AddressPostcode.ShouldBe("Person One Address Postcode");

                personViewModels[1].Id.ShouldBe(person2.PersonId);
                personViewModels[1].Name.ShouldBe("Person Two");
                personViewModels[1].AddressId.ShouldBeNull();
                personViewModels[1].AddressLine1.ShouldBeNull();
                personViewModels[1].AddressLine2.ShouldBeNull();
                personViewModels[1].AddressPostcode.ShouldBeNull();
            });
        }
    }
}
