namespace AgileObjects.AgileMapper.Configuration
{
    internal interface IReverseConflictable
    {
        bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem);
    }
}