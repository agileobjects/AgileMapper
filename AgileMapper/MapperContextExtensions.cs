namespace AgileObjects.AgileMapper
{
    using System;
    using Caching;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MapperContextExtensions
    {
        public static Expression GetIdentifierOrNull(
            this MapperContext context,
            Expression subject,
            ICache<TypeKey, Expression> cache = null)
        {
            var typeIdsCache = cache ?? context.Cache.CreateScoped<TypeKey, Expression>(default(HashCodeComparer<TypeKey>));

            return typeIdsCache.GetOrAdd(TypeKey.ForTypeId(subject.Type), key =>
            {
                var configuredIdentifier =
                    context.UserConfigurations.Identifiers.GetIdentifierOrNullFor(key.Type);

                if (configuredIdentifier != null)
                {
                    return configuredIdentifier.ReplaceParameterWith(subject);
                }

                var identifier = context.GetIdentifierOrNull(key.Type);

                return identifier?.GetAccess(subject);
            });
        }

        public static Member GetIdentifierOrNull(this MapperContext context, Type type)
            => context.Naming.GetIdentifierOrNull(type);

        public static Expression GetValueConversion(this MapperContext context, Expression value, Type targetType)
            => context.ValueConverters.GetConversion(value, targetType);
    }
}
