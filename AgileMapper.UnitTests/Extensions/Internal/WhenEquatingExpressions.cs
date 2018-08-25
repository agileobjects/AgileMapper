namespace AgileObjects.AgileMapper.UnitTests.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Dlr = Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenEquatingExpressions
    {
        [Fact]
        public void ShouldEquateCheckedAdditions()
        {
            Expression<Func<int, int, int>> bindingsOne = (x, y) => checked(x + y);
            Expression<Func<int, int, int>> bindingsTwo = (x, y) => checked(x + y);

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateCheckedSubtractions()
        {
            Expression<Func<int, int, int>> bindingsOne = (x, y) => checked(x - y);
            Expression<Func<int, int, int>> bindingsTwo = (x, y) => checked(x - y);

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateCheckedMultiplications()
        {
            Expression<Func<int, int, int>> bindingsOne = (x, y) => checked(x * y);
            Expression<Func<int, int, int>> bindingsTwo = (x, y) => checked(x * y);

            VerifyEqual(bindingsOne, bindingsTwo);
        }
        [Fact]
        public void ShouldEquateAModuloOperation()
        {
            Expression<Func<int, int, bool>> bindingsOne = (x, y) => x % y == 0;
            Expression<Func<int, int, bool>> bindingsTwo = (x, y) => x % y == 0;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateNegatedDefaultComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => !(x > default(int));
            Expression<Func<int, bool>> bindingsTwo = x => !(x > default(int));

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateTypeIsComparisons()
        {
            Expression<Func<object, bool>> bindingsOne = x => x is Person;
            Expression<Func<object, bool>> bindingsTwo = x => x is Person;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateTypeAsComparisons()
        {
            Expression<Func<object, Person>> bindingsOne = x => x as Person;
            Expression<Func<object, Person>> bindingsTwo = x => x as Person;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateAndComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => x > 0 && x < 100;
            Expression<Func<int, bool>> bindingsTwo = x => x > 0 && x < 100;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateBitwiseAndComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => x > 0 & x < 100;
            Expression<Func<int, bool>> bindingsTwo = x => x > 0 & x < 100;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateOrComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => x > 0 || x < 100;
            Expression<Func<int, bool>> bindingsTwo = x => x > 0 || x < 100;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateBitwiseOrComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => x > 0 | x < 100;
            Expression<Func<int, bool>> bindingsTwo = x => x > 0 | x < 100;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateExclusiveOrComparisons()
        {
            Expression<Func<int, bool>> bindingsOne = x => x > 0 ^ x < 100;
            Expression<Func<int, bool>> bindingsTwo = x => x > 0 ^ x < 100;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateMemberAccesses()
        {
            Expression<Func<MemberExpression, ExpressionType>> accessOne = m => m.Expression.NodeType;
            Expression<Func<MemberExpression, ExpressionType>> accessTwo = m => m.Expression.NodeType;

            VerifyEqual(accessOne, accessTwo);
        }

        [Fact]
        public void ShouldEquivalateMemberAccesses()
        {
            Expression<Func<MemberExpression, ExpressionType>> accessOne = m => m.Expression.NodeType;
            Expression<Func<Expression, ExpressionType>> accessTwo = e => e.NodeType;

            VerifyEquivalent(accessOne, accessTwo);
        }

        [Fact]
        public void ShouldEquateListInitialisations()
        {
            Expression<Func<List<int>>> bindingsOne = () => new List<int> { 1, 2, 3 };
            Expression<Func<List<int>>> bindingsTwo = () => new List<int> { 1, 2, 3 };

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateBitwiseLeftShiftOperations()
        {
            Expression<Func<int, int, int>> bindingsOne = (x, y) => x << y;
            Expression<Func<int, int, int>> bindingsTwo = (x, y) => x << y;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateBitwiseRightShiftOperations()
        {
            Expression<Func<int, int, int>> bindingsOne = (x, y) => x >> y;
            Expression<Func<int, int, int>> bindingsTwo = (x, y) => x >> y;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateCoalesceOperations()
        {
            Expression<Func<int?, int, int>> bindingsOne = (x, y) => x ?? y;
            Expression<Func<int?, int, int>> bindingsTwo = (x, y) => x ?? y;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateNegationOperations()
        {
            Expression<Func<int, int>> bindingsOne = x => -x;
            Expression<Func<int, int>> bindingsTwo = x => -x;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateCheckedNegationOperations()
        {
            Expression<Func<int, int>> bindingsOne = x => checked(-x);
            Expression<Func<int, int>> bindingsTwo = x => checked(-x);

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateANewBoundedArrayCreation()
        {
            Expression<Func<int, int[]>> bindingsOne = x => new int[x];
            Expression<Func<int, int[]>> bindingsTwo = x => new int[x];

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateArrayIndexAccesses()
        {
            Expression<Func<int[], int>> bindingsOne = x => x[0];
            Expression<Func<int[], int>> bindingsTwo = x => x[0];

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateArrayLengthAccesses()
        {
            Expression<Func<int[], bool>> bindingsOne = x => x.Length == 1;
            Expression<Func<int[], bool>> bindingsTwo = x => x.Length == 1;

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldEquateListBindings()
        {
            Expression<Func<PublicField<List<int>>>> bindingsOne = () => new PublicField<List<int>> { Value = { 1 } };
            Expression<Func<PublicField<List<int>>>> bindingsTwo = () => new PublicField<List<int>> { Value = { 1 } };

            VerifyEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldDifferentiateListInitialisationsByInitialisationCount()
        {
            Expression<Func<List<int>>> bindingsOne = () => new List<int> { 1, 2, 3 };
            Expression<Func<List<int>>> bindingsTwo = () => new List<int> { 1, 2 };

            VerifyNotEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldDifferentiateExpressionsByListBindingInitialiserCount()
        {
            Expression<Func<PublicField<List<int>>>> bindingsOne = () => new PublicField<List<int>> { Value = { 1 } };
            Expression<Func<PublicField<List<int>>>> bindingsTwo = () => new PublicField<List<int>> { Value = { 1, 2 } };

            VerifyNotEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldDifferentiateExpressionsByBindingCount()
        {
            Expression<Func<Address>> oneBinding = () => new Address { Line1 = "One!" };
            Expression<Func<Address>> twoBindings = () => new Address { Line1 = "One!", Line2 = "Two!" };

            VerifyNotEqual(oneBinding, twoBindings);
        }

        [Fact]
        public void ShouldDifferentiateExpressionsByBindingType()
        {
            Expression<Func<PublicTwoFields<string, List<string>>>> bindingsOne = () => new PublicTwoFields<string, List<string>>
            {
                Value1 = "One!",
                Value2 = { "Two!" }
            };

            Expression<Func<PublicTwoFields<string, List<string>>>> bindingsTwo = () => new PublicTwoFields<string, List<string>>
            {
                Value2 = { "Two!" },
                Value1 = "One!"
            };

            VerifyNotEqual(bindingsOne, bindingsTwo);
        }

        [Fact]
        public void ShouldDifferentiateExpressionsBySubBindings()
        {
            Expression<Func<Person>> bindingsOne = () => new Person { Address = { Line1 = "One!" } };
            Expression<Func<Person>> bindingsTwo = () => new Person { Address = { Line1 = "One!", Line2 = "Two!" } };

            VerifyNotEqual(bindingsOne, bindingsTwo);
        }

        private static void VerifyEqual(Expression one, Expression two) => Verify(one, two).ShouldBeTrue();

        private static void VerifyNotEqual(Expression one, Expression two) => Verify(one, two).ShouldBeFalse();

        private static bool Verify(Expression one, Expression two)
        {
#if NET35
            return ExpressionEvaluation.AreEqual(one.ToDlrExpression(), two.ToDlrExpression());
#else
            return ExpressionEvaluation.AreEqual(one, two);
#endif
        }

        private static void VerifyEquivalent(Expression one, Expression two)
        {
#if NET35
            ExpressionEvaluation.AreEquivalent(one.ToDlrExpression(), two.ToDlrExpression()).ShouldBeTrue();
#else
            ExpressionEvaluation.AreEquivalent(one, two).ShouldBeTrue();
#endif        
        }
    }
}
