namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal enum CallbackPosition
    {
        Before,
        After
    }

    internal static class CallbackPositionExtensions
    {
        public static bool IsPriorToObjectCreation(this CallbackPosition? position, QualifiedMember targetMember)
        {
            if (position != CallbackPosition.Before)
            {
                return false;
            }

            return (targetMember == QualifiedMember.All) || (targetMember == QualifiedMember.None);
        }
    }
}