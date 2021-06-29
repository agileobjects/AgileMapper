namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenFlatteningViaExtensionMethods
    {
#if !NET35
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

                var result = source.FlattenUsing(mapper).ToDynamic();

                ((string)result._0Line1).ShouldBe("1_1");
                ((string)result._0Line2).ShouldBe("1_2");
                ((string)result._1Line1).ShouldBe("2_1");
                ((string)result._1Line2).ShouldBe("2_2");
                ((string)result._2Line1).ShouldBe("3_1");
                ((string)result._2Line2).ShouldBe("3_2");
            }
        }

        [Fact]
        public void ShouldFlattenToDynamicWithInlineConfiguration()
        {
            var source = new[]
            {
                new Address { Line1 = "1_1", Line2 = "1_2" },
                new Address { Line1 = "2_1", Line2 = "2_2" },
                new Address { Line1 = "3_1", Line2 = "3_2" }
            };

            var result = source.Flatten().ToDynamic(cfg => cfg
                .ForDynamics
                .UseElementKeyPattern("_i_"));

            ((string)result._0__Line1).ShouldBe("1_1");
            ((string)result._0__Line2).ShouldBe("1_2");
            ((string)result._1__Line1).ShouldBe("2_1");
            ((string)result._1__Line2).ShouldBe("2_2");
            ((string)result._2__Line1).ShouldBe("3_1");
            ((string)result._2__Line2).ShouldBe("3_2");
        }

        [Fact]
        public void ShouldFlattenToDynamicWithInlineConfigurationAndASpecifiedMapper()
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

                var result = source.FlattenUsing(mapper).ToDynamic(cfg => cfg
                    .ForDynamics
                    .UseElementKeyPattern("_i_"));

                ((string)result._0_Line1).ShouldBe("1_1");
                ((string)result._0_Line2).ShouldBe("1_2");
                ((string)result._1_Line1).ShouldBe("2_1");
                ((string)result._1_Line2).ShouldBe("2_2");
                ((string)result._2_Line1).ShouldBe("3_1");
                ((string)result._2_Line2).ShouldBe("3_2");
            }
        }
#endif
        [Fact]
        public void ShouldFlattenToDictionary()
        {
            var source = new[]
            {
                new Address { Line1 = "1_1", Line2 = "1_2" },
                new Address { Line1 = "2_1", Line2 = "2_2" },
                new Address { Line1 = "3_1", Line2 = "3_2" }
            };

            var result = source.Flatten().ToDictionary();

            result["[0].Line1"].ShouldBe("1_1");
            result["[0].Line2"].ShouldBe("1_2");
            result["[1].Line1"].ShouldBe("2_1");
            result["[1].Line2"].ShouldBe("2_2");
            result["[2].Line1"].ShouldBe("3_1");
            result["[2].Line2"].ShouldBe("3_2");
        }

        [Fact]
        public void ShouldFlattenToDictionaryWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address[]>()
                    .ToDictionaries
                    .UseElementKeyPattern("-i-")
                    .UseFlattenedMemberNames();

                var source = new[]
                {
                    new Address { Line1 = "1_1", Line2 = "1_2" },
                    new Address { Line1 = "2_1", Line2 = "2_2" },
                    new Address { Line1 = "3_1", Line2 = "3_2" }
                };

                var result = source.FlattenUsing(mapper).ToDictionary();

                result["-0-Line1"].ShouldBe("1_1");
                result["-0-Line2"].ShouldBe("1_2");
                result["-1-Line1"].ShouldBe("2_1");
                result["-1-Line2"].ShouldBe("2_2");
                result["-2-Line1"].ShouldBe("3_1");
                result["-2-Line2"].ShouldBe("3_2");
            }
        }

        [Fact]
        public void ShouldFlattenAnEnumerableToDictionaryWithInlineConfiguration()
        {
            var source = new[]
            {
                new PublicTwoFieldsStruct<string, string> { Value1 = "1_1", Value2 = "1_2" },
                new PublicTwoFieldsStruct<string, string> { Value1 = "2_1", Value2 = "2_2" }
            };

            var result = source.Flatten().ToDictionary(
                cfg => cfg
                    .WhenMapping
                    .Dictionaries
                    .UseFlattenedTargetMemberNames()
                    .UseElementKeyPattern("<i>"),
                cfg => cfg
                    .WhenMapping
                    .From<PublicTwoFieldsStruct<string, string>>()
                    .ToDictionaries
                    .MapMember(ptfs => ptfs.Value1)
                    .ToMemberNameKey("V1"));

            result["<0>V1"].ShouldBe("1_1");
            result["<0>Value2"].ShouldBe("1_2");
            result["<1>V1"].ShouldBe("2_1");
            result["<1>Value2"].ShouldBe("2_2");
        }

        [Fact]
        public void ShouldFlattenToDictionaryWithInlineConfigurationAndASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address[]>()
                    .ToDictionaries
                    .UseFlattenedMemberNames();

                var source = new[]
                {
                    new Address { Line1 = "1_1", Line2 = "1_2" },
                    new Address { Line1 = "2_1", Line2 = "2_2" }
                };

                var result = source.FlattenUsing(mapper).ToDictionary(cfg => cfg
                    .ForDictionaries
                    .UseElementKeyPattern("(i)"));

                result.ShouldContainKeyAndValue("(0)Line1", "1_1");
                result.ShouldContainKeyAndValue("(0)Line2", "1_2");
                result.ShouldContainKeyAndValue("(1)Line1", "2_1");
                result.ShouldContainKeyAndValue("(1)Line2", "2_2");
            }
        }

        [Fact]
        public void ShouldFlattenToStringDictionary()
        {
            var source = new[]
            {
                new Address { Line1 = "1_1", Line2 = "1_2" },
                new Address { Line1 = "2_1", Line2 = "2_2" },
                new Address { Line1 = "3_1", Line2 = "3_2" }
            };

            var result = source.Flatten().ToDictionary<string>();

            result["[0].Line1"].ShouldBe("1_1");
            result["[0].Line2"].ShouldBe("1_2");
            result["[1].Line1"].ShouldBe("2_1");
            result["[1].Line2"].ShouldBe("2_2");
            result["[2].Line1"].ShouldBe("3_1");
            result["[2].Line2"].ShouldBe("3_2");
        }

        [Fact]
        public void ShouldFlattenToStringDictionaryWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address[]>()
                    .ToDictionaries
                    .UseElementKeyPattern("-i-")
                    .UseFlattenedMemberNames();

                var source = new[]
                {
                    new Address { Line1 = "1_1", Line2 = "1_2" },
                    new Address { Line1 = "2_1", Line2 = "2_2" },
                    new Address { Line1 = "3_1", Line2 = "3_2" }
                };

                var result = source.FlattenUsing(mapper).ToDictionary<string>();

                result["-0-Line1"].ShouldBe("1_1");
                result["-0-Line2"].ShouldBe("1_2");
                result["-1-Line1"].ShouldBe("2_1");
                result["-1-Line2"].ShouldBe("2_2");
                result["-2-Line1"].ShouldBe("3_1");
                result["-2-Line2"].ShouldBe("3_2");
            }
        }
    }
}
