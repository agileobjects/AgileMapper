namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Extensions.Internal;
    using Api.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringObjectMappingInline
    {
        [Fact]
        public void ShouldUseAConfiguredFactoryForARootMappingInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new Address { Line1 = "Over here", Line2 = "Over there" };

                var result = mapper.Map(source).ToANew<Address>(cfg => cfg
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1 + "!",
                        Line2 = ctx.Source.Line2 + "!"
                    }));

                result.Line1.ShouldBe("Over here!");
                result.Line2.ShouldBe("Over there!");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryForARootMappingConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingSource = new Address { Line1 = "Over here" };

                var matchingResult = mapper.Map(matchingSource).ToANew<Address>(cfg => cfg
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1,
                        Line2 = ctx.Source.Line1 + " again"
                    }));

                matchingResult.Line1.ShouldBe("Over here");
                matchingResult.Line2.ShouldBe("Over here again");

                var nonMatchingSource = new Address { Line1 = "Over here", Line2 = "Over there" };

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Address>(cfg => cfg
                    .If(ctx => string.IsNullOrEmpty(ctx.Source.Line2))
                    .MapInstancesUsing(ctx => new Address
                    {
                        Line1 = ctx.Source.Line1,
                        Line2 = ctx.Source.Line1 + " again"
                    }));

                nonMatchingResult.Line1.ShouldBe("Over here");
                nonMatchingResult.Line2.ShouldBe("Over there");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldUseAConfiguedThreeParameterFactoryForARootArrayElementMappingInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Func<Address[], Address[], int?, Address> addressMapper = (srcAddrs, tgtAddrs, index) =>
                {
                    var sourceAddress = srcAddrs[index.GetValueOrDefault()];

                    return new Address
                    {
                        Line1 = (index + 1) + " " + sourceAddress.Line1,
                        Line2 = sourceAddress.Line2
                    };
                };

                var source = new[]
                {
                    new Address { Line1 = "Line 1.1", Line2 = "Line 1.2" },
                    new Address { Line1 = "Line 2.1" }
                };

                var result = mapper.Map(source).ToANew<Address[]>(cfg => cfg
                    .MapInstancesOf<Address>().Using(addressMapper));

                result.Length.ShouldBe(2);

                result.First().Line1.ShouldBe("1 Line 1.1");
                result.First().Line2.ShouldBe("Line 1.2");

                result.Second().Line1.ShouldBe("2 Line 2.1");
                result.Second().Line2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldUseAConfiguredFactoryForARootCollectionMappingConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                Expression<Action<IFullMappingInlineConfigurator<ICollection<int>, ICollection<int>>>> config =
                    cfg => cfg
                        .If(ctx => ctx.Source.Count == 3)
                        .MapInstancesUsing(ctx => ctx.Source
                            .Select((item, index) => item + index)
                            .ToList());

                ICollection<int> matchingSource = new[] { 1, 1, 1 };
                var matchingResult = mapper.Map(matchingSource).ToANew(config);

                matchingResult.ShouldBe(1, 2, 3);

                ICollection<int> nonMatchingSource = new[] { 4, 5, 6, 7 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew(config);

                nonMatchingResult.ShouldBe(4, 5, 6, 7);
            }
        }

        [Fact]
        public void ShouldUseAConfiguredNonEnumerableToEnumerableFactoryForANestedArrayMappingInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new PublicTwoFields<string, Address>
                {
                    Value1 = "Hello!",
                    Value2 = new Address { Line1 = "Line 1" }
                };

                var result = mapper.Map(source).ToANew<PublicTwoFields<string, Address[]>>(cfg => cfg
                    .WhenMapping
                        .From<Address>()
                        .To<Address[]>()
                        .MapInstancesUsing(ctx => new[]
                        {
                            new Address { Line1 = ctx.Source.Line1 },
                            new Address { Line1 = ctx.Source.Line1 + " again" }
                        }));

                result.Value1.ShouldBe("Hello!");
                result.Value2.ShouldNotBeNull().Length.ShouldBe(2);
                result.Value2.First().Line1.ShouldBe("Line 1");
                result.Value2.Second().Line1.ShouldBe("Line 1 again");
            }
        }
    }
}
