namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms;
    using TestClasses;
    using Xunit;

    public class WhenProjectingFlatTypes : WhenProjectingFlatTypes<EfCore2TestDbContext>
    {
        public WhenProjectingFlatTypes(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldProjectStructCtorParameters() => RunShouldProjectStructCtorParameters();

        [Fact]
        public Task ShouldVaryMappersByProviderType()
        {
            return RunTest(async (context, mapper) =>
            {
                await context.BoolItems.AddAsync(new PublicBool { Value = true });
                await context.SaveChangesAsync();

                await context
                    .BoolItems
                    .ProjectUsing(mapper).To<PublicBoolDto>()
                    .FirstOrDefaultAsync();

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                new PublicBoolQueryable()
                    .ProjectUsing(mapper).To<PublicBoolDto>()
                    .FirstOrDefault();

                mapper.RootMapperCountShouldBe(2);
            });
        }

        #region Helper Class

        private class PublicBoolQueryable : IQueryable<PublicBool>
        {
            private readonly PublicBoolProvider _provider;
            private readonly IQueryable<PublicBool> _values;

            public PublicBoolQueryable()
            {
                _provider = new PublicBoolProvider();
                _values = new PublicBool[0].AsQueryable();
            }

            IEnumerator<PublicBool> IEnumerable<PublicBool>.GetEnumerator()
                => _values.GetEnumerator();

            public IEnumerator GetEnumerator() => _values.GetEnumerator();

            public Expression Expression => Expression.Constant(_values);

            public Type ElementType => typeof(PublicBool);

            public IQueryProvider Provider => _provider;
        }

        private class PublicBoolProvider : IQueryProvider
        {
            public IQueryable CreateQuery(Expression expression)
                => Enumerable.Empty<PublicBool>().AsQueryable();

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => Enumerable.Empty<PublicBool>().Cast<TElement>().AsQueryable();

            public object Execute(Expression expression) => default(PublicBool);

            public TResult Execute<TResult>(Expression expression) => default(TResult);
        }

        #endregion
    }
}