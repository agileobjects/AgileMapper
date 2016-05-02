namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Extensions;
    using AgileMapper.Members;

    public abstract class MemberFinderTestsBase
    {
        private static readonly MemberFinder _memberFinder = new MemberFinder();

        internal QualifiedMember SourceMemberFor<T>(T sourceObject)
        {
            var sourceParameter = Parameters.Create<T>("source");
            var sourceProperty = typeof(T).GetProperties(Constants.PublicInstance).First();
            var sourcePropertyAccess = Expression.Property(sourceParameter, sourceProperty);
            var sourcePropertyCastToObject = sourcePropertyAccess.GetConversionTo(typeof(object));
            var sourcePropertyLambda = Expression.Lambda<Func<T, object>>(sourcePropertyCastToObject, sourceParameter);

            return SourceMemberFor(sourcePropertyLambda);
        }

        internal QualifiedMember SourceMemberFor<T>(Expression<Func<T, object>> childMemberExpression = null)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.From(Member.RootSource(typeof(T)))
                : childMemberExpression.ToSourceMember(_memberFinder);
        }

        internal QualifiedMember TargetMemberFor<T>(Expression<Func<T, object>> childMemberExpression = null)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.From(Member.RootTarget(typeof(T)))
                : childMemberExpression.ToTargetMember(_memberFinder);
        }
    }
}