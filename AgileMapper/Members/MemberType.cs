namespace AgileObjects.AgileMapper.Members
{
    internal enum MemberType
    {
        ConstructorParameter,
        Field,
        Property,
        GetMethod,
        SetMethod,
        EnumerableElement
    }

    internal static class MemberTypeExtensions
    {
        public static bool IsReadable(this MemberType memberType)
        {
            switch (memberType)
            {
                case MemberType.ConstructorParameter:
                case MemberType.SetMethod:
                    return false;
            }

            return true;
        }
    }
}