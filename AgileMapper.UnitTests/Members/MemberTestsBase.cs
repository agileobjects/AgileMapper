namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Extensions.Internal;
    using AgileMapper.Members;
    using AgileMapper.Members.Extensions;
    using NetStandardPolyfills;

    public abstract class MemberTestsBase
    {
        internal static readonly MapperContext DefaultMapperContext = new();
        internal static readonly MemberCache MemberCache = GlobalContext.Instance.MemberCache;

        internal IQualifiedMember SourceMemberFor<T>(T sourceObject)
        {
            var sourceParameter = typeof(T).GetOrCreateSourceParameter();
            var sourceProperty = typeof(T).GetPublicInstanceProperties().First();
            var sourcePropertyAccess = Expression.Property(sourceParameter, sourceProperty);
            var sourcePropertyCastToObject = Expression.Convert(sourcePropertyAccess, typeof(object));
            var sourcePropertyLambda = Expression.Lambda<Func<T, object>>(sourcePropertyCastToObject, sourceParameter);

            return SourceMemberFor(sourceObject, sourcePropertyLambda);
        }

        internal IQualifiedMember SourceMemberFor<T>(T sourceObject, Expression<Func<T, object>> childMemberExpression)
            => SourceMemberFor(Member.RootSource<T>(), childMemberExpression);

        internal static IQualifiedMember SourceMemberFor(Member rootSourceMember, Expression childMemberExpression)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.CreateRoot(rootSourceMember, DefaultMapperContext)
                : childMemberExpression.ToSourceMember(DefaultMapperContext);
        }

        internal QualifiedMember TargetMemberFor<T>(Expression<Func<T, object>> childMemberExpression = null)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.CreateRoot(Member.RootTarget(typeof(T)), DefaultMapperContext)
                : childMemberExpression.ToTargetMember(DefaultMapperContext);
        }
    }
}