namespace AgileObjects.AgileMapper
{
    using System;

    internal interface ITypePair
    {
        Type SourceType { get; }

        Type TargetType { get; }

        bool IsForSourceType(ITypePair typePair);

        bool IsForTargetType(ITypePair typePair);

        bool HasCompatibleTypes(ITypePair typePair);
    }
}