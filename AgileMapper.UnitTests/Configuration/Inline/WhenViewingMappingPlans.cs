namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldApplyAnExpressionConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<Person>()
                    .Over<PersonViewModel>(cfg => cfg
                        .Map((p, pvm) => p.Title + " " + p.Name)
                        .To(pvm => pvm.Name));

                plan.ShouldContain("pToPvmData.Target.Name = sourcePerson.Title + \" \" + sourcePerson.Name");

                var result = mapper
                    .Map(new Person { Title = Title.Count, Name = "Dooko" })
                    .Over(new PersonViewModel());

                result.Name.ShouldBe("Count Dooko");
            }
        }

        [Fact]
        public void ShouldApplyAnIgnoredMemberConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<Person>()
                    .ToANew<PersonViewModel>(cfg => cfg
                        .Ignore(pvm => pvm.AddressLine1));

                plan.ShouldContain("// AddressLine1 is ignored");

                var result = mapper
                    .Map(new Customer { Name = "Luke", Address = new Address { Line1 = "Far, Far Away" } })
                    .ToANew<PersonViewModel>();

                result.Name.ShouldBe("Luke");
                result.AddressLine1.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldCombineApiAndInlineConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .OnTo<CustomerViewModel>()
                    .Map((c, cvm) => c.Title + " " + c.Name)
                    .To(cvm => cvm.Name);

                string plan = mapper
                    .GetPlanFor<Customer>()
                    .OnTo<CustomerViewModel>(cfg => cfg
                        .Ignore(cvm => cvm.AddressLine1));

                plan.ShouldContain("cToCvmData.Target.Name = sourceCustomer.Title + \" \" + sourceCustomer.Name");
                plan.ShouldContain("// AddressLine1 is ignored");

                var result = mapper
                    .Map(new Customer
                    {
                        Title = Title.Dr,
                        Name = "Vader",
                        Address = new Address { Line1 = "Far, Far Away" }
                    })
                    .OnTo(new CustomerViewModel());

                result.Name.ShouldBe("Dr Vader");
                result.AddressLine1.ShouldBeNull();
            }
        }
    }
}
