namespace AgileObjects.AgileMapper.Configuration
{
    internal interface IPotentialClone
    {
        bool IsClone { get; }

        bool IsInlineConfiguration { get; set; }

        IPotentialClone Clone();

        bool IsReplacementFor(IPotentialClone clonedItem);
    }
}