namespace AgileObjects.AgileMapper.UnitTests.Dynamics.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Shouldly;
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

        [Fact]
        public void ShouldNotApplySourceOnlyConfigurationToTargetDynamics()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDynamics
                    .UseFlattenedTargetMemberNames();

                var source = new Customer
                {
                    Name = "Paul",
                    Address = new Address { Line1 = "Abbey Road", Line2 = "Penny Lane" }
                };

                dynamic target = new ExpandoObject();

                target.Name = "Ringo";

                mapper.Map(source).OnTo(target);

                ((string)target.Name).ShouldBe("Ringo");
                ((string)target.Address_Line1).ShouldBe("Abbey Road");
                ((string)target.Address_Line2).ShouldBe("Penny Lane");
            }
        }

        [Fact]
        public void ShouldApplyFlattenedMemberNamesToASpecificSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .ToDynamics
                    .UseFlattenedMemberNames()
                    .And
                    .MapMember(pf => pf.Value)
                    .ToMemberName("Data");

                var matchingSource = new PublicField<Address>
                {
                    Value = new Address { Line1 = "As a pancake" }
                };
                var matchingResult = mapper.Map(matchingSource).ToANew<dynamic>();

                ((IDictionary<string, object>)matchingResult).Keys.Any(k => k.StartsWith("Value")).ShouldBeFalse();
                ((string)matchingResult.DataLine1).ShouldBe("As a pancake");
                ((string)matchingResult.DataLine2).ShouldBeNull();

                var nonMatchingSource = new PublicProperty<Address>
                {
                    Value = new Address { Line1 = "Like a flatfish" }
                };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<dynamic>();

                ((string)nonMatchingResult.Value_Line1).ShouldBe("Like a flatfish");
                ((string)nonMatchingResult.Value_Line2).ShouldBeNull();
            }
        }
    }
}
