namespace AgileObjects.AgileMapper.Configuration
{
    internal interface IPotentialClone
    {
        bool IsClone { get; }

        IPotentialClone Clone();

        bool IsReplacementFor(IPotentialClone clonedItem);
    }
}