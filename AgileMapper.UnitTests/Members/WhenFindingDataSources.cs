namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Members;
    using Common;
    using Common.TestClasses;
    using ObjectPopulation;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenFindingDataSources : MemberTestsBase
    {
        [Fact]
        public void ShouldNotMatchSameNameIncompatibleTypeProperties()
        {
            var source = new TwoValues { Value = new int[5], value = string.Empty };
            var target = new PublicProperty<byte>();

            var matchingSourceMember = GetMatchingSourceMember(source, target, pp => pp.Value);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("value");
        }

        [Fact]
        public void ShouldUseBaseClassMembers()
        {
            var source = new Derived { Value = 123 };
            var target = new PublicProperty<int>();

            var matchingSourceMember = GetMatchingSourceMember(source, target, pp => pp.Value);

            matchingSourceMember.ShouldNotBeNull();
            matchingSourceMember.Name.ShouldBe("Value");
        }

        private IQualifiedMember GetMatchingSourceMember<TSource, TTarget>(
            TSource source,
            TTarget target,
            Expression<Func<TTarget, object>> childMemberExpression)
        {
            var targetMember = TargetMemberFor(childMemberExpression);

            var mappingContext = new SimpleMappingContext(DefaultMapperContext.RuleSets.CreateNew, DefaultMapperContext);
            var rootMappingData = ObjectMappingDataFactory.ForRoot(source, target, MappingTypes<TSource, TTarget>.Fixed, mappingContext);
            var rootMapperData = rootMappingData.MapperData;

            var childMapperData = new ChildMemberMapperData(targetMember, rootMapperData);
            var childMappingContext = rootMappingData.GetChildMappingData(childMapperData);

            return SourceMemberMatcher.GetMatchFor(childMappingContext).SourceMember;
        }

        #region Helper Classes

        private class TwoValues
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once NotAccessedField.Local
            public string value;

            public TwoValues()
            {
                value = string.Empty;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int[] Value { get; set; }
        }

        private abstract class Base
        {
            public virtual int Value { get; set; }
        }

        private class Derived : Base
        {
        }

        #endregion
    }
}