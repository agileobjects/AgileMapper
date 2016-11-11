// Largely based on the AutoMapper benchmark project:
// Project: https://github.com/AutoMapper/AutoMapper/tree/master/src/Benchmark
// Licence: https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt

namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using AbstractMappers;
    using ConcreteMappers.AgileMapper;
    using ConcreteMappers.AutoMapper;
    using ConcreteMappers.ExpressMapper;
    using ConcreteMappers.Manual;
    using ConcreteMappers.Mapster;
    using ConcreteMappers.ValueInjecter;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            var mapperSets = new[]
            {
                new IObjectMapper[]
                {
                    new ManualCtorMapper(),
                    new AgileMapperCtorMapper(),
                    new AutoMapperCtorMapper(),
                    new ExpressMapperCtorMapper(),
                    new MapsterCtorMapper(),
                    new ValueInjecterCtorMapper()
                },
                new IObjectMapper[]
                {
                    new ManualComplexTypeMapper(),
                    new AgileMapperComplexTypeMapper(),
                    new AutoMapperComplexTypeMapper(),
                    new ExpressMapperComplexTypeMapper(),
                    new MapsterComplexTypeMapper(),
                    new ValueInjecterComplexTypeMapper()
                },
                new IObjectMapper[]
                {
                    new ManualFlatteningMapper(),
                    new AgileMapperFlatteningMapper(),
                    new AutoMapperFlatteningMapper(),
                    new ExpressMapperFlatteningMapper(),
                    new MapsterFlatteningMapper(),
                    new ValueInjecterFlatteningMapper()
                },
                new IObjectMapper[]
                {
                    new ManualUnflatteningMapper(),
                    new AgileMapperUnflatteningMapper(),
                    //new AutoMapperUnflatteningMapper(),    // Not supported
                    //new ExpressMapperUnflatteningMapper(), // Not supported
                    //new MapsterUnflatteningMapper(),       // Not supported
                    new ValueInjecterUnflatteningMapper()
                },
                new IObjectMapper[]
                {
                    new ManualDeepMapper(),
                    new AgileMapperDeepMapper(),
                    new AutoMapperDeepMapper(),
                    new ExpressMapperDeepMapper(),
                    new MapsterDeepMapper(),
                    new ValueInjecterDeepMapper()
                }
            };

            foreach (var mapperSet in mapperSets)
            {
                MapperTester.Test(mapperSet);
                Console.WriteLine();
            }

            Console.WriteLine("Done!");
        }
    }
}
