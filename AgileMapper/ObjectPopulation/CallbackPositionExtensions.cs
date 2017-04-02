namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Diagnostics;
    using Members;

    internal static class CallbackPositionExtensions
    {
        [DebuggerStepThrough]
        public static bool IsPriorToObjectCreation(this CallbackPosition? position, QualifiedMember targetMember)
            => IsPriorToObjectCreation(position.GetValueOrDefault(), targetMember);

        [DebuggerStepThrough]
        public static bool IsPriorToObjectCreation(this CallbackPosition position, QualifiedMember targetMember)
        {
            if (position != CallbackPosition.Before)
            {
                return false;
            }

            return (targetMember == QualifiedMember.All) || (targetMember == QualifiedMember.None);
        }
    }
}