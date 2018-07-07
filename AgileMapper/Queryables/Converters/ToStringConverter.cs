namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class ToStringConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (IsNotToStringCall(methodCall))
            {
                converted = null;
                return false;
            }

            if (modifier.Settings.SupportsToString)
            {
                var convertedCall = AdjustForFormatStringIfNecessary(methodCall, modifier);
                converted = GetNullCheckedToStringCall(convertedCall, modifier);

                return (convertedCall != methodCall) || (converted != convertedCall);
            }

            methodCall = AdjustForFormatStringIfNecessary(methodCall, modifier);
            converted = modifier.Settings.ConvertToStringCall(methodCall);
            return true;
        }

        private static bool IsNotToStringCall(MethodCallExpression methodCall)
            => methodCall.Method.IsStatic || (methodCall.Method.Name != nameof(ToString));

        private static MethodCallExpression AdjustForFormatStringIfNecessary(
            MethodCallExpression toStringCall,
            IQueryProjectionModifier modifier)
        {
            if (modifier.Settings.SupportsToStringWithFormat || toStringCall.Arguments.None())
            {
                return toStringCall;
            }

            return toStringCall.Object.WithToStringCall();
        }

        private static Expression GetNullCheckedToStringCall(
            MethodCallExpression toStringCall,
            IQueryProjectionModifier modifier)
        {
            // ReSharper disable once PossibleNullReferenceException
            var subjectNonNullableType = toStringCall.Object.Type.GetNonNullableType();

            if (subjectNonNullableType.IsEnum())
            {
                return GetNullCheckedEnumToStringCall(toStringCall, subjectNonNullableType, modifier);
            }

            return GetNullCheckedToStringCall(toStringCall, subjectNonNullableType);
        }

        private static Expression GetNullCheckedEnumToStringCall(
            MethodCallExpression toStringCall,
            Type subjectNonNullableType,
            IQueryProjectionModifier modifier)
        {
            if (modifier.Settings.SupportsEnumToStringConversion)
            {
                return GetNullCheckedToStringCall(toStringCall, subjectNonNullableType);
            }

            return GetNullCheckedToStringCall(
                toStringCall.Object,
                Enum.GetUnderlyingType(subjectNonNullableType));
        }

        private static Expression GetNullCheckedToStringCall(
            MethodCallExpression toStringCall,
            Type subjectNonNullableType)
        {
            var toStringSubject = GetToStringSubject(toStringCall);

            return (toStringSubject.Type != subjectNonNullableType)
                ? GetNullCheckedToStringCall(toStringSubject, subjectNonNullableType)
                : toStringCall;
        }

        private static Expression GetToStringSubject(MethodCallExpression toStringCall)
        {
            var toStringSubject = toStringCall.Object;

            // ReSharper disable once PossibleNullReferenceException
            if (toStringSubject.NodeType == ExpressionType.MemberAccess)
            {
                var memberAccess = (MemberExpression)toStringSubject;

                if ((memberAccess.Member.Name == nameof(Nullable<int>.Value)) &&
                     memberAccess.Expression.Type.IsNullableType())
                {
                    toStringSubject = memberAccess.Expression;
                }
            }

            return toStringSubject;
        }

        private static Expression GetNullCheckedToStringCall(
            Expression toStringSubject,
            Type subjectNonNullableType)
        {
            var checkedConversion = NullableConversionConverter.GetNullCheckedConversion(
                toStringSubject,
                Expression.Convert(toStringSubject, subjectNonNullableType).WithToStringCall());

            return checkedConversion;
        }
    }
}