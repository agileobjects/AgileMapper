namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions;
    using Settings;

    internal static class EmptyArrayConverter
    {
        public static bool TryConvert(
            NewArrayExpression newArray,
            IQueryProviderSettings settings,
            out Expression converted)
        {
            if (settings.SupportsEmptyArrayCreation || IsNotEmptyArrayCreation(newArray))
            {
                converted = null;
                return false;
            }

            converted = settings.ConvertEmptyArrayCreation(newArray);
            return true;
        }

        private static bool IsNotEmptyArrayCreation(NewArrayExpression newArray)
        {
            return !newArray.Expressions.HasOne() ||
                   (newArray.Expressions[0].NodeType != ExpressionType.Constant) ||
                 (((ConstantExpression)newArray.Expressions[0]).Value != (object)0);
        }
    }
}