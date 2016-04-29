namespace AgileObjects.AgileMapper.UnitTests.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using AgileMapper.Extensions;
    using Shouldly;
    using TestClasses;
    using Xunit;

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
        public void ShouldEvaluateAComplexTypeAsComplex()
        {
            typeof(Person).IsComplex().ShouldBeTrue();
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

        #region CanBeNull

        [Fact]
        public void ShouldEvaluateAComplexTypeAsPotentiallyNull()
        {
            typeof(Person).CanBeNull().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAnArrayAsPotentiallyNull()
        {
            typeof(string[]).CanBeNull().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAGenericEnumerableAsPotentiallyNull()
        {
            typeof(IEnumerable<DateTime>).CanBeNull().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAStringAsPotentiallyNull()
        {
            typeof(string).CanBeNull().ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotEvaluateAGuidAsPotentiallyNull()
        {
            typeof(Guid).CanBeNull().ShouldBeFalse();
        }

        [Fact]
        public void ShouldEvaluateANullableValueTypeAsPotentiallyNull()
        {
            typeof(long?).CanBeNull().ShouldBeTrue();
        }

        #endregion
    }
}
