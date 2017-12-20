namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface ITypePair
    {
        Type SourceType { get; }

        Type TargetType { get; }
    }
}