﻿namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Runtime.InteropServices;
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenFlatteningViaExtensionMethods
    {
        [Fact]
        public void ShouldFlattenToDynamic()
        {
            var source = new[]
            {
                new Address { Line1 = "1_1", Line2 = "1_2" },
                new Address { Line1 = "2_1", Line2 = "2_2" },
                new Address { Line1 = "3_1", Line2 = "3_2" }
            };

            var result = source.Flatten().ToDynamic();

            ((string)result._0_Line1).ShouldBe("1_1");
            ((string)result._0_Line2).ShouldBe("1_2");
            ((string)result._1_Line1).ShouldBe("2_1");
            ((string)result._1_Line2).ShouldBe("2_2");
            ((string)result._2_Line1).ShouldBe("3_1");
            ((string)result._2_Line2).ShouldBe("3_2");
        }

        [Fact]
        public void ShouldFlattenToDynamicWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address[]>()
                    .ToDynamics
                    .UseFlattenedMemberNames();

                var source = new[]
                {
                    new Address { Line1 = "1_1", Line2 = "1_2" },
                    new Address { Line1 = "2_1", Line2 = "2_2" },
                    new Address { Line1 = "3_1", Line2 = "3_2" }
                };

                var result = source.Flatten(_ => _.Using(mapper)).ToDynamic();

                ((string)result._0Line1).ShouldBe("1_1");
                ((string)result._0Line2).ShouldBe("1_2");
                ((string)result._1Line1).ShouldBe("2_1");
                ((string)result._1Line2).ShouldBe("2_2");
                ((string)result._2Line1).ShouldBe("3_1");
                ((string)result._2Line2).ShouldBe("3_2");
            }
        }
    }
}
