namespace AgileObjects.AgileMapper
{
    using System;

    internal interface ITypePair
    {
        Type SourceType { get; }

        Type TargetType { get; }
        
        bool HasCompatibleTypes(ITypePair typePair);
    }
}