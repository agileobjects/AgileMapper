namespace AgileObjects.AgileMapper
{
    using System;

    internal interface ITypePair
    {
        Type SourceType { get; }

        Type TargetType { get; }
    }

    internal interface IChild<out TParent>
    {
        TParent Parent { get; }
    }
}