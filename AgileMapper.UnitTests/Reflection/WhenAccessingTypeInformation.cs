namespace AgileObjects.AgileMapper.UnitTests.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenAccessingTypeInformation
    {
        #region IsEnumerable

        [Fact]
        public void ShouldEvaluateAnArrayAsEnumerable()
        {
            typeof(int[]).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAGenericListAsEnumerable()
        {
            typeof(List<string>).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateANonGenericListAsEnumerable()
        {
            typeof(IList).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotEvaluateAStringAsEnumerable()
        {
            typeof(string).IsEnumerable().ShouldBeFalse();
        }

        #endregion

        #region IsComplex

        [Fact]
        public void ShouldEvaluateAClassAsComplex()
        {
            typeof(Person).IsComplex().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAStructAsComplex()
        {
            typeof(PublicCtorStruct<>).IsComplex().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAnInterfaceAsComplex()
        {
            typeof(IPublicInterface<>).IsComplex().ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotEvaluateAnArrayAsComplex()
        {
            typeof(string[]).IsComplex().ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotEvaluateAGenericCollectionAsComplex()
        {
            typeof(ICollection<byte>).IsComplex().ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotEvaluateAStringAsComplex()
        {
            typeof(string).IsComplex().ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotEvaluateAValueTypeAsComplex()
        {
            typeof(int).IsComplex().ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotEvaluateAGuidAsComplex()
        {
            typeof(Guid).IsComplex().ShouldBeFalse();
        }

        [Fact]
        public void ShouldNotEvaluateANullableValueTypeAsComplex()
        {
            typeof(double?).IsComplex().ShouldBeFalse();
        }

        #endregion

        [Fact]
        public void ShouldEvaluateATypeAsFromTheBcl()
        {
            typeof(Func<>).IsFromBcl().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateATypeAsNotFromTheBcl()
        {
            typeof(WhenAccessingTypeInformation).IsFromBcl().ShouldBeFalse();
        }
    }
}
