namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    internal interface IBasicConstructionInfo
    {
        bool IsConfigured { get; }
            
        bool IsUnconditional { get; }
            
        int ParameterCount { get; }
            
        int Priority { get; }
    }
}