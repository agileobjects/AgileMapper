#if NET35
namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Reflection;
    using Extensions.Internal;
    using Microsoft.Scripting.Ast;
    using NetStandardPolyfills;

    internal class ToGuidConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new ToGuidConverter();

        private static readonly MethodInfo _parseGuidMethod = typeof(StringExtensions).GetPublicStaticMethod("ToGuid");
        private static readonly MethodInfo _parseGuidNullableMethod = typeof(StringExtensions).GetPublicStaticMethod("ToGuidNullable");

        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return
                (nonNullableTargetType == typeof(Guid)) &&
               ((nonNullableSourceType == typeof(Guid)) ||
                 ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType));
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(Guid?))
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            var parseMethod = targetType == typeof(Guid)
                ? _parseGuidMethod
                : _parseGuidNullableMethod;

            var parseCall = Expression.Call(parseMethod, sourceValue);

            return parseCall;
        }
    }
}
#endif