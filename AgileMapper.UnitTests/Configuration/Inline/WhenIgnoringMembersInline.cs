namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembersInline
    {
        [Fact]
        public void ShouldIgnoreMultipleConfiguredMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var id = Guid.NewGuid();

                var matchingResult = mapper
                    .Map(new
                    {
                        Id = id.ToString(),
                        Name = "Bilbo",
                        AddressLine1 = "House Street",
                        AddressLine2 = "Town City"
                    })
                    .ToANew<Customer>(cfg => cfg
                        .Ignore(c => c.Discount, c => c.Address.Line2));

                matchingResult.Id.ShouldBe(id);
                matchingResult.Name.ShouldBe("Bilbo");
                matchingResult.Discount.ShouldBeDefault();
                matchingResult.Address.ShouldNotBeNull();
                matchingResult.Address.Line1.ShouldBe("House Street");
                matchingResult.Address.Line2.ShouldBeNull();
            }
        }
    }
}
