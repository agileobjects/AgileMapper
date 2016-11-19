namespace AgileObjects.AgileMapper.UnitTests.Members
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AgileMapper.Extensions;
    using AgileMapper.Members;
    using NetStandardPolyfills;

    public abstract class MemberTestsBase
    {
        internal static readonly MapperContext DefaultMapperContext = new MapperContext();
        internal static readonly MemberFinder MemberFinder = GlobalContext.Instance.MemberFinder;

        internal IQualifiedMember SourceMemberFor<T>(T sourceObject)
        {
            var sourceParameter = Parameters.Create<T>("source");
            var sourceProperty = typeof(T).GetPublicInstanceProperties().First();
            var sourcePropertyAccess = Expression.Property(sourceParameter, sourceProperty);
            var sourcePropertyCastToObject = sourcePropertyAccess.GetConversionTo(typeof(object));
            var sourcePropertyLambda = Expression.Lambda<Func<T, object>>(sourcePropertyCastToObject, sourceParameter);

            return SourceMemberFor(sourceObject, sourcePropertyLambda);
        }

        internal IQualifiedMember SourceMemberFor<T>(T sourceObject, Expression<Func<T, object>> childMemberExpression)
            => SourceMemberFor(Member.RootSource<T>(), childMemberExpression);

        internal IQualifiedMember SourceMemberFor<T>(Expression<Func<T, object>> childMemberExpression = null)
            => SourceMemberFor(Member.RootSource<T>(), childMemberExpression);

        private static IQualifiedMember SourceMemberFor(Member rootSourceMember, LambdaExpression childMemberExpression)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.From(rootSourceMember, MapperContext.WithDefaultNamingSettings)
                : MemberExtensions.CreateMember(
                    childMemberExpression,
                    Member.RootSource,
                    MemberFinder.GetReadableMembers,
                    MapperContext.WithDefaultNamingSettings);
        }

        internal QualifiedMember TargetMemberFor<T>(Expression<Func<T, object>> childMemberExpression = null)
        {
            return (childMemberExpression == null)
                ? QualifiedMember.From(Member.RootTarget(typeof(T)), MapperContext.WithDefaultNamingSettings)
                : childMemberExpression.ToTargetMember(MapperContext.WithDefaultNamingSettings);
        }
    }
}