namespace AgileObjects.AgileMapper.Extensions
{
    using ReadableExpressions.Extensions;

    internal static class TypeInfo<T>
    {
        public static readonly bool IsEnumerable = typeof(T).IsEnumerable();
        public static readonly bool IsSimple = typeof(T).IsSimple();
        // ReSharper disable StaticMemberInGenericType
        public static readonly bool CheckSourceType = IsSourceTypeCheckRequired();
        public static readonly bool CheckTargetType = IsTargetTypeCheckRequired();
        // ReSharper restore StaticMemberInGenericType

        private static bool IsSourceTypeCheckRequired()
        {
            var type = typeof(T);

            // TODO: IsSimple check might be required:
            if (type.IsSealed() || IsSimple)
            {
                return false;
            }

            if (IsEnumerable)
            {
                return !type.IsGenericType();
            }

            return true;
        }

        private static bool IsTargetTypeCheckRequired()
        {
            var type = typeof(T);

            // TODO: IsSimple check might be required:
            if (type.IsSealed() || IsSimple)
            {
                return false;
            }

            return type.IsInterface() || !IsEnumerable;
        }
    }
}