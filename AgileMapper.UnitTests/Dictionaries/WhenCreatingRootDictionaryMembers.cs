namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System.Collections.Generic;
    using AgileMapper.Members;
    using AgileMapper.Members.Dictionaries;
    using Members;
    using TestClasses;
    using Xunit;

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
