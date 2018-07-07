namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using AgileMapper.Members;
    using AgileMapper.Members.Dictionaries;
    using Members;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCreatingRootDictionaryMembers : MemberTestsBase
    {
        [Fact]
        public void ShouldVarySourceMembersByTargetType()
        {
            var memberFactory = new QualifiedMemberFactory(DefaultMapperContext);

            var dictionaryToPersonArraySourceMember = memberFactory
                .RootSource<Dictionary<string, Person[]>, Person[]>()
                as DictionarySourceMember;

            dictionaryToPersonArraySourceMember.ShouldNotBeNull();

            // ReSharper disable once PossibleNullReferenceException
            dictionaryToPersonArraySourceMember.CouldContainSourceInstance.ShouldBeTrue();

            var dictionaryToObjectSourceMember = memberFactory
                .RootSource<Dictionary<string, Person[]>, PersonViewModel[]>()
                as DictionarySourceMember;

            dictionaryToObjectSourceMember.ShouldNotBeSameAs(dictionaryToPersonArraySourceMember);
        }
    }
}
