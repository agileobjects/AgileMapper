namespace AgileObjects.AgileMapper.UnitTests.Orms.EfCore2.Configuration
{
    using System;
    using System.Threading.Tasks;
    using AgileMapper.Members;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Orms.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringDataSources : WhenConfiguringDataSources<EfCore2TestDbContext>
    {
        public WhenConfiguringDataSources(InMemoryEfCore2TestContext context)
            : base(context)
        {
        }

        [Fact]
        public Task ShouldApplyAConfiguredMember() => DoShouldApplyAConfiguredMember();

        [Fact]
        public Task ShouldApplyMultipleConfiguredMembers() => DoShouldApplyMultipleConfiguredMembers();

        [Fact]
        public Task ShouldUseMappingDataConfiguredSourceOnlyDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicStringDto>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 3 });
                await context.SaveChangesAsync();

                var stringDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicStringDto>()
                    .FirstAsync();

                stringDto.Value.ShouldBe("6");
            });
        }

        [Fact]
        public Task ShouldUseConfiguredSourceOnlyDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicStringDto>()
                    .Map((s, t, i) => s.Value * 3)
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 3 });
                await context.SaveChangesAsync();

                var stringDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicStringDto>()
                    .FirstAsync();

                stringDto.Value.ShouldBe("9");
            });
        }

        [Fact]
        public Task ShouldUseMethodInvocationDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .Map((s, t, i) => MultiplyByFive(s))
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 5 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(25);
            });
        }

        [Fact]
        public Task ShouldNotUseMappingDataConfiguredSourceAndTargetDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .Map(ctx => ctx.Source.Value * ctx.Target.Value)
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 3 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(3);
            });
        }

        [Fact]
        public Task ShouldNotUseConfiguredSourceAndTargetDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .Map((s, t) => s.Value - t.Value)
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 2 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(2);
            });
        }

        [Fact]
        public Task ShouldNotUseConfiguredFuncDataSource()
        {
            return RunTest(async (context, mapper) =>
            {
                Func<IMappingData<PublicInt, PublicIntDto>, int> sumValues =
                    ctx => ctx.Source.Value + ctx.Target.Value;

                mapper.WhenMapping
                    .From<PublicInt>()
                    .To<PublicIntDto>()
                    .Map(sumValues)
                    .To(dto => dto.Value);

                await context.IntItems.AddAsync(new PublicInt { Value = 3 });
                await context.SaveChangesAsync();

                var intDto = await context
                    .IntItems
                    .ProjectUsing(mapper)
                    .To<PublicIntDto>()
                    .FirstAsync();

                intDto.Value.ShouldBe(3);
            });
        }

        #region Helper Members

        private static int MultiplyByFive(PublicInt publicInt) => publicInt.Value * 5;

        #endregion
    }
}