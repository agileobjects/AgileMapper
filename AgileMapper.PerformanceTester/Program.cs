// Largely based on the AutoMapper benchmark project:
// Project: https://github.com/AutoMapper/AutoMapper/tree/master/src/Benchmark
// Licence: https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt

namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Linq;
    using AbstractMappers;
    using ConcreteMappers.AgileMapper;
    using ConcreteMappers.AutoMapper;
    using ConcreteMappers.ExpressMapper;
    using ConcreteMappers.Manual;
    using ConcreteMappers.Mapster;
    using ConcreteMappers.ValueInjecter;

    // Specify comma-separated sets of mapper Ids and (optionally) test Ids from the string arrays
    // below if desired. e.g:
    //  - Run the deep and complex tests for AgileMapper:
    //    ag deep,compl
    //  - Run the constructor and flattening mapping tests for the manual and ExpressMapper mappers:
    //    man,exp ctor,flat

    public class Program
    {
        private static readonly string[] _mapperIds = { "man", "ag", "au", "exp", "ma", "vi" };
        private static readonly string[] _testIds = { "ctor", "compl", "flat", "unflat", "deep" };

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

            string[] mappersToTest, testsToRun;

            if (!TryGetMappersToTest(args, out mappersToTest))
            {
                Console.WriteLine(
                    "Invalid mapper(s) specified: {0}{1}Available mapper ids: {2}",
                    string.Join(", ", mappersToTest),
                    Environment.NewLine,
                    string.Join(", ", _mapperIds));
            }

            if (!TryGetTestsToRun(args, out testsToRun))
            {
                Console.WriteLine(
                    "Invalid test(s) specified: {0}{1}Available tests: {2}",
                    string.Join(", ", testsToRun),
                    Environment.NewLine,
                    string.Join(", ", _testIds));
            }

            for (var i = 0; i < mapperSets.Length; i++)
            {
                if (!testsToRun.Contains(_testIds[i]))
                {
                    continue;
                }

                var relevantMappers = (mappersToTest == _mapperIds)
                    ? mapperSets[i]
                    : mapperSets[i]
                        .Select((ms, j) => new
                        {
                            MapperSet = ms,
                            Index = j
                        })
                        .Where(ms => mappersToTest.Contains(_mapperIds[ms.Index]))
                        .Select(ms => ms.MapperSet)
                        .ToArray();

                MapperTester.Test(relevantMappers);
                Console.WriteLine();
            }

            Console.WriteLine("Done!");
        }

        private static bool TryGetMappersToTest(string[] args, out string[] mappersToTest)
        {
            if (args.Length == 0)
            {
                mappersToTest = _mapperIds;
                return true;
            }

            var mapperIds = args[0].ToLowerInvariant().Split(',');
            var invalidMapperIds = mapperIds.Except(_mapperIds).ToArray();

            if (invalidMapperIds.Any())
            {
                mappersToTest = invalidMapperIds;
                return false;
            }

            mappersToTest = _mapperIds.Intersect(mapperIds).ToArray();
            return true;
        }

        private static bool TryGetTestsToRun(string[] args, out string[] testsToRun)
        {
            if (args.Length < 2)
            {
                testsToRun = _testIds;
                return true;
            }

            var testIds = args[1].ToLowerInvariant().Split(',');
            var invalidTestIds = testIds.Except(_testIds).ToArray();

            if (invalidTestIds.Any())
            {
                testsToRun = invalidTestIds;
                return false;
            }

            testsToRun = _testIds.Intersect(testIds).ToArray();
            return true;
        }
    }
}
