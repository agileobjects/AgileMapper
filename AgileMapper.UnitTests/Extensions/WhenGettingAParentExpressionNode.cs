namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Extensions;
    using Shouldly;
    using TestClasses;
    using Xunit;

    // ReSharper disable PossibleNullReferenceException
    public class WhenGettingAParentExpressionNode
    {
        [Fact]
        public void ShouldReturnAMemberAccessParent()
        {
            Expression<Func<PersonViewModel, string>> personViewModelName = pvm => pvm.Name;

            var namePropertyParent = personViewModelName.Body.GetParentOrNull() as ParameterExpression;

            namePropertyParent.ShouldNotBeNull();
            namePropertyParent.Name.ShouldBe("pvm");
        }

        [Fact]
        public void ShouldReturnANestedMemberAccessParent()
        {
            Expression<Func<Person, string>> personAddressLine1 = p => p.Address.Line1;

            var addressLine1PropertyParent = personAddressLine1.Body.GetParentOrNull() as MemberExpression;

            addressLine1PropertyParent.ShouldNotBeNull();
            addressLine1PropertyParent.Member.Name.ShouldBe("Address");
        }

        [Fact]
        public void ShouldReturnAMemberMethodCallParent()
        {
            Expression<Func<Person, string>> personAddressToString = p => p.Address.ToString();

            var addressToStringPropertyParent = personAddressToString.Body.GetParentOrNull() as MemberExpression;

            addressToStringPropertyParent.ShouldNotBeNull();
            addressToStringPropertyParent.Member.Name.ShouldBe("Address");
        }

        [Fact]
        public void ShouldReturnAnExtensionMethodCallParent()
        {
            Expression<Func<Person[], Person[]>> personAddressToArray = ps => ps.ToArray();

            var addressToArrayPropertyParent = personAddressToArray.Body.GetParentOrNull() as ParameterExpression;

            addressToArrayPropertyParent.ShouldNotBeNull();
            addressToArrayPropertyParent.Name.ShouldBe("ps");
        }
    }
    // ReSharper restore PossibleNullReferenceException
}
