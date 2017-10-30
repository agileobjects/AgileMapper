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
    }
}
