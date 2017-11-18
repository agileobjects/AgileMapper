namespace AgileObjects.AgileMapper.UnitTests.Orms.SimpleTypeConversion
{
    using System;
    using System.Linq;
    using Infrastructure;
    using Shouldly;
    using TestClasses;

    public abstract class WhenConvertingToGuids<TOrmContext> : OrmTestClassBase<TOrmContext>
        where TOrmContext : ITestDbContext, new()
    {
        protected WhenConvertingToGuids(ITestContext<TOrmContext> context)
            : base(context)
        {
        }

        #region Parseable String -> Guid

        protected void RunShouldProjectAParseableStringToAGuid()
            => RunTest(ProjectAParseableStringToAGuid);

        protected void RunShouldErrorProjectingAParseableStringToAGuid()
            => RunTestAndExpectThrow(ProjectAParseableStringToAGuid);

        private static void ProjectAParseableStringToAGuid(TOrmContext context)
        {
            var guid = Guid.NewGuid();

            context.StringItems.Add(new PublicString { Value = guid.ToString() });
            context.SaveChanges();

            var guidItem = context.StringItems.ProjectTo<PublicGuidDto>().First();

            guidItem.Value.ShouldBe(guid);
        }

        #endregion

        #region Null String -> Guid

        protected void RunShouldProjectANullStringToAGuid()
            => RunTest(ProjectANullStringToAGuid);

        protected void RunShouldErrorProjectingANullStringToAGuid()
            => RunTestAndExpectThrow(ProjectANullStringToAGuid);

        private static void ProjectANullStringToAGuid(TOrmContext context)
        {
            context.StringItems.Add(new PublicString { Value = default(string) });
            context.SaveChanges();

            var guidItem = context.StringItems.ProjectTo<PublicGuidDto>().First();

            guidItem.Value.ShouldBe(default(Guid));
        }

        #endregion
    }
}
