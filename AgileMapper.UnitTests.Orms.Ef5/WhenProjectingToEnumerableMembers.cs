﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.Ef5
{
    using Enumerables;
    using Infrastructure;
    using Xunit;

    public class WhenProjectingToEnumerableMembers :
        WhenProjectingToEnumerableMembers<Ef5TestDbContext>,
        ICollectionMemberProjectionFailureTest,
        IEnumerableMemberProjectorTest
    {
        public WhenProjectingToEnumerableMembers(InMemoryEf5TestContext context)
            : base(context)
        {
        }

        [Fact]
        public void ShouldErrorProjectingToAComplexTypeCollectionMember()
            => RunShouldErrorProjectingToAComplexTypeCollectionMember();

        [Fact]
        public void ShouldProjectToAComplexTypeEnumerableMember()
            => RunShouldProjectToAComplexTypeEnumerableMember();
    }
}