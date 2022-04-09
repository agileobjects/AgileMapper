namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Configuration;
    using Extensions.Internal;
    using Members;

    internal static class MapperContextExtensions
    {
        public static IList<DerivedTypePair> GetDerivedTypePairs(this IQualifiedMemberContext context)
        {
            return context
                .MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(context);
        }

        public static Expression GetIdentifierOrNull(
            this MapperContext context,
            Expression subject,
            ICache<TypeKey, Expression> cache = null)
        {
            cache ??= context.Cache.CreateScopedWithHashCodes<TypeKey, Expression>();

            return cache.GetOrAdd(TypeKey.ForTypeId(subject.Type), key =>
            {
                var configuredIdentifier =
                    context.UserConfigurations.Identifiers.GetIdentifierOrNullFor(key.Type);

                if (configuredIdentifier != null)
                {
                    return configuredIdentifier.ReplaceParameterWith(subject);
                }

                var identifier = context.GetIdentifierOrNull(key.Type);

                return identifier?.GetReadAccess(subject);
            });
        }

        public static Member GetIdentifierOrNull(this MapperContext context, Type type)
            => context.Naming.GetIdentifierOrNull(type);

        public static TService GetServiceOrThrow<TService>(
            this IMapperContextOwner mapperContextOwner,
            string name)
            where TService : class
        {
            return mapperContextOwner.MapperContext.UserConfigurations
                .GetServiceOrThrow<TService>(name);
        }

        public static TServiceProvider GetServiceProviderOrThrow<TServiceProvider>(
            this IMapperContextOwner mapperContextOwner)
            where TServiceProvider : class
        {
            return mapperContextOwner.MapperContext.UserConfigurations
                .GetServiceProviderOrThrow<TServiceProvider>();
        }
    }
}
