namespace AgileObjects.AgileMapper.Configuration
{
    internal interface IPotentialAutoCreatedItem
    {
        bool WasAutoCreated { get; }

        IPotentialAutoCreatedItem Clone();

        bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem);
    }
}