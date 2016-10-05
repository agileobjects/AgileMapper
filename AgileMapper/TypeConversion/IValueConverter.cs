namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Implementing classes will convert a value type or string to another value type.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IValueConverter"/> converts the given 
        /// <paramref name="nonNullableTargetType"/>.
        /// </summary>
        /// <param name="nonNullableTargetType">The non-nullable type to evaluate.</param>
        /// <returns>
        /// True if the <see cref="IValueConverter"/> converts the given 
        /// <paramref name="nonNullableTargetType"/>, otherwise false.
        /// </returns>
        bool IsFor(Type nonNullableTargetType);

        /// <summary>
        /// Gets a value indicating whether the <see cref="IValueConverter"/> can convert the given 
        /// <paramref name="nonNullableSourceType"/> to the target type it handles.
        /// </summary>
        /// <param name="nonNullableSourceType">The non-nullable type to evaluate.</param>
        /// <returns>
        /// True if the <see cref="IValueConverter"/> can convert the given 
        /// <paramref name="nonNullableSourceType"/> to the target type it handles, otherwise false.
        /// </returns>
        bool CanConvert(Type nonNullableSourceType);

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