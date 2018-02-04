namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using TestClasses;

    public abstract class WhenConvertingToGuids<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToGuids(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Parseable String -> Guid

        protected Task RunShouldProjectAParseableStringToAGuid()
            => RunTest(ProjectAParseableStringToAGuid);

        protected Task RunShouldErrorProjectingAParseableStringToAGuid()
            => RunTestAndExpectThrow(ProjectAParseableStringToAGuid);

        private static async Task ProjectAParseableStringToAGuid(TOrmContext context)
        {
            var guid = Guid.NewGuid();

            context.StringItems.Add(new PublicString { Value = guid.ToString() });
            await context.SaveChanges();

            var guidItem = context.StringItems.Project().To<PublicGuidDto>().First();

            guidItem.Value.ShouldBe(guid);
        }

        #endregion

        #region Null String -> Guid

        protected Task RunShouldProjectANullStringToAGuid()
            => RunTest(ProjectANullStringToAGuid);

        protected Task RunShouldErrorProjectingANullStringToAGuid()
            => RunTestAndExpectThrow(ProjectANullStringToAGuid);

        private static async Task ProjectANullStringToAGuid(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            await context.SaveChanges();

            var guidItem = context.StringItems.Project().To<PublicGuidDto>().First();

            guidItem.Value.ShouldBe(default(Guid));
        }

        #endregion
    }
}
