namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using Members;

    internal interface IBasicConstructionInfo
    {
        bool IsConfigured { get; }

        bool IsUnconditional { get; }

        int ParameterCount { get; }

        int Priority { get; }

        bool HasCtorParameterFor(Member targetMember);
    }
}