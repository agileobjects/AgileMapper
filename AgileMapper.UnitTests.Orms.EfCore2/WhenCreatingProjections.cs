namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Infrastructure;
    using TestClasses;
    using Xunit;

    public class WhenCreatingProjections : OrmTestClassBase<EfCore2TestDbContext>
    {
        public WhenCreatingProjections(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldReuseACachedProjectionMapper()
        {
            return RunTest(async mapper =>
            {
                using (var context1 = new EfCore2TestDbContext())
                {
                    var stringDtos = await context1
                        .StringItems
                        .ProjectUsing(mapper).To<PublicStringDto>()
                        .ToListAsync();

                    stringDtos.ShouldBeEmpty();
                }

                mapper.RootMapperCountShouldBeOne();

                using (var context2 = new EfCore2TestDbContext())
                {
                    context2.StringItems.Add(new PublicString { Id = 1, Value = "New!" });
                    await context2.SaveChangesAsync();

                    var moreStringDtos = await context2
                        .StringItems
                        .ProjectUsing(mapper).To<PublicStringDto>()
                        .ToArrayAsync();

                    moreStringDtos.ShouldHaveSingleItem();
                }

                mapper.RootMapperCountShouldBeOne();
            });
        }

        [Fact]
        public Task ShouldMapAQueryableAsAnEnumerable()
        {
            return RunTest(async (context, mapper) =>
            {
                await context.BoolItems.AddRangeAsync(new PublicBool { Value = true }, new PublicBool { Value = false });
                await context.SaveChangesAsync();

                var result = mapper
                    .Map(context.BoolItems.Where(bi => bi.Value))
                    .ToANew<List<PublicBoolDto>>();

                result.ShouldNotBeNull();
                result.ShouldHaveSingleItem().Value.ShouldBeTrue();
            });
        }

        // See https://github.com/agileobjects/AgileMapper/issues/204
        [Fact]
        public void ShouldHandleEnumerableQueryableMappingUnmappableElements()
        {
            var source =
                new[] { new PublicBool { Value = true } }
                    .AsQueryable().Where(b => b.Value);

            var result = source.Project().To<Issue204.Dto>().First();

            result.ShouldNotBeNull().CanWrite.ShouldBeFalse();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/204
        [Fact]
        public void ShouldIgnoreConfiguredServiceProviderInEnumerableQueryableMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .UseServiceProvider(new Issue204.ServiceProvider());

                mapper.WhenMapping
                    .From<Issue204.Entity>()
                    .To<Issue204.EntityDto>()
                    .Map(ctx => ctx.GetService<Issue204.User>().HasAccess())
                    .To(dto => dto.CanWrite);

                var source =
                    new List<Issue204.Entity> { new Issue204.Entity { Id = 1 } }
                        .AsQueryable().Where(e => e.Id == 1);

                var result = source.ProjectUsing(mapper).To<Issue204.EntityDto>().First();

                result.ShouldNotBeNull();
                result.Id.ShouldBe(1);
                result.CanWrite.ShouldBeFalse();
            }
        }

        #region Helper Classes

        private static class Issue204
        {
            public class ServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                    => Activator.CreateInstance(serviceType);
            }

            public class User
            {
                public bool HasAccess() => true;
            }

            public class Entity
            {
                public long Id { get; set; }
            }

            public class Dto
            {
                public bool CanWrite { get; set; }
            }

            public class EntityDto
            {
                public long Id { get; set; }

                public bool CanWrite { get; set; }
            }
        }

        #endregion
    }
}
