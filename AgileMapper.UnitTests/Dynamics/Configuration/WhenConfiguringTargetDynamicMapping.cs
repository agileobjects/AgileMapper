namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringTargetDynamicMapping
    {
        [Fact]
        public void ShouldApplyFlattenedMemberNamesGlobally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .Dynamics
                    .UseFlattenedTargetMemberNames();

                var source = new MysteryCustomer
                {
                    Id = Guid.NewGuid(),
                    Title = Title.Mr,
                    Name = "Paul",
                    Discount = 0.25m,
                    Report = "Naah nah nah na-na-na NAAAAAAAHHHH",
                    Address = new Address { Line1 = "Abbey Road", Line2 = "Penny Lane" }
                };
                var result = mapper.Map(source).ToANew<dynamic>();

                ((Guid)result.Id).ShouldBe(source.Id);
                ((Title)result.Title).ShouldBe(Title.Mr);
                ((string)result.Name).ShouldBe("Paul");
                ((decimal)result.Discount).ShouldBe(0.25m);
                ((string)result.AddressLine1).ShouldBe("Abbey Road");
                ((string)result.AddressLine2).ShouldBe("Penny Lane");
            }
        }
    }
}
