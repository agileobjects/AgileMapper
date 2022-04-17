namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;

    [Flags]
    internal enum LambdaValue
    {
        MappingContext = 1,
        Parent = 2,
        Source = 4,
        Target = 8,
        CreatedObject = 16,
        ElementIndex = 32,
        ElementKey = 64,
        ServiceProvider = 128
    }
}