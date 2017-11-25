﻿namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System;
    using System.Linq;
    using Infrastructure;
    using MoreTestClasses;
    using ObjectPopulation;
    using Queryables.Api;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public abstract class WhenViewingMappingPlans<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenViewingMappingPlans(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldCreateAProjectionMappingPlanForASpecificQueryProvider()
        {
            RunTest(mapper =>
            {
                string plan = mapper
                    .GetPlanFor<Product>()
                    .ProjectedTo<ProductDto>(GetQueryProviderType);

                plan.ShouldContain("Rule Set: Project");
                plan.ShouldContain("Source.Select(");
                plan.ShouldContain("new ProductDto");

                var cachedMapper = (IObjectMapper)mapper.RootMapperCountShouldBeOne();

                cachedMapper.MapperData.SourceType.ShouldBe(typeof(IQueryable<Product>));
                cachedMapper.MapperData.TargetType.ShouldBe(typeof(IQueryable<ProductDto>));

                // Trigger a mapping:
                Context.Products.ProjectTo<ProductDto>().ShouldBeEmpty();

                var usedMapper = (IObjectMapper)mapper.RootMapperCountShouldBeOne();

                usedMapper.ShouldBe(cachedMapper);

            });
        }

        protected abstract Type GetQueryProviderType(QueryProviderTypeSelector selector);
    }
}
