namespace AgileObjects.AgileMapper
{
    using System;
    using System.Reflection;

    internal static class Constants
    {
        public static readonly Type[] NoTypeArguments = { };

        public static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        public static readonly BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    }
}
