namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembers : WhenIgnoringMembers<EfCore2TestDbContext>
    {
        public WhenIgnoringMembers(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldNotUseSourceAndTargetConfiguredIgnoreCondition()
        {
            return RunTest(async (context, mapper) =>
            {
                var product1 = new Product { Name = "P.1" };
                var product2 = new Product { Name = "P.2" };

                await context.Products.AddRangeAsync(product1, product2);
                await context.SaveChangesAsync();

                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .If((p, d) => d.Name.EndsWith("1"))
                    .Ignore(p => p.Name);

                var productDtos = context
                    .Products
                    .ProjectUsing(mapper).To<ProductDto>()
                    .OrderBy(p => p.ProductId)
                    .ToArray();

                productDtos.First().ProductId.ShouldBe(product1.ProductId);
                productDtos.First().Name.ShouldBe("P.1");
                productDtos.Second().ProductId.ShouldBe(product2.ProductId);
                productDtos.Second().Name.ShouldBe("P.2");
            });
        }

        [Fact]
        public Task ShouldNotUseConfiguredFuncIgnoreCondition()
        {
            return RunTest(async (context, mapper) =>
            {
                Func<PublicInt, PublicIntDto, bool> targetGreaterThanOne = (s, t) => t.Value > 1;

                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .If((s, t) => targetGreaterThanOne(s, t))
                    .Ignore(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 4 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(4);
            });
        }
    }
}
