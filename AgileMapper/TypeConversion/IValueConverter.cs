namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Implementing classes will convert a value type or string to another value type.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IValueConverter"/> converts values of the given 
        /// <paramref name="nonNullableSourceType"/> to the given <paramref name="nonNullableTargetType"/>.
        /// </summary>
        /// <param name="nonNullableSourceType">The non-nullable type to evaluate.</param>
        /// <param name="nonNullableTargetType">The non-nullable type to evaluate.</param>
        /// <returns>
        /// True if the <see cref="IValueConverter"/> converts the given 
        /// <paramref name="nonNullableTargetType"/>, otherwise false.
        /// </returns>
        bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType);

        /// <summary>
        /// Gets an Expression converting the given <paramref name="sourceValue"/> to the given 
        /// <paramref name="targetType"/>.
        /// </summary>
        /// <param name="sourceValue">The source value to convert.</param>
        /// <param name="targetType">The target type to which to convert to <paramref name="sourceValue"/>.</param>
        /// <returns>
        /// An Expression converting the given <paramref name="sourceValue"/> to the given 
        /// <paramref name="targetType"/>.
        /// </returns>
        Expression GetConversion(Expression sourceValue, Type targetType);
    }
}