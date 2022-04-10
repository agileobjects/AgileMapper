namespace AgileObjects.AgileMapper.UnitTests.Reflection
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenAccessingTypeInformation
    {
        #region IsComplex

        [Fact, Trait("Category", "Checked")]
        public void ShouldEvaluateAClassAsComplex()
        {
            typeof(Person).IsComplex().ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldEvaluateAStructAsComplex()
        {
            typeof(PublicCtorStruct<>).IsComplex().ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldEvaluateAnInterfaceAsComplex()
        {
            typeof(IPublicInterface<>).IsComplex().ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateAnArrayAsComplex()
        {
            typeof(string[]).IsComplex().ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateAGenericCollectionAsComplex()
        {
            typeof(ICollection<byte>).IsComplex().ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateAStringAsComplex()
        {
            typeof(string).IsComplex().ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateAValueTypeAsComplex()
        {
            typeof(int).IsComplex().ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateAGuidAsComplex()
        {
            typeof(Guid).IsComplex().ShouldBeFalse();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldNotEvaluateANullableValueTypeAsComplex()
        {
            typeof(double?).IsComplex().ShouldBeFalse();
        }

        #endregion

        [Fact, Trait("Category", "Checked")]
        public void ShouldEvaluateATypeAsFromTheBcl()
        {
            typeof(Func<>).IsFromBcl().ShouldBeTrue();
        }

        [Fact, Trait("Category", "Checked")]
        public void ShouldEvaluateATypeAsNotFromTheBcl()
        {
            typeof(WhenAccessingTypeInformation).IsFromBcl().ShouldBeFalse();
        }
    }
}
