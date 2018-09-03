// Largely based on the AutoMapper benchmark project:
// Project: https://github.com/AutoMapper/AutoMapper/tree/master/src/Benchmark
// Licence: https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt

// AgileMapper and AutoMapper perform better with this attribute applied,
// but ExpressMapper and Mapster throw exceptions - uncomment to test the
// Agile and Auto at their fastest:
//[assembly: System.Security.AllowPartiallyTrustedCallers]

namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Collections.Generic;
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
        private static readonly string[] _testIds = { "ctor", "compl", "compls", "flat", "unflat", "unflats", "deep", "deeps", "ent", "ents", "new" };

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            if (!TryGetMappersToTest(args, out var mappersToTest))
            {
                Console.WriteLine(
                    "Invalid mapper(s) specified: {0}{1}Available mapper ids: {2}",
                    string.Join(", ", mappersToTest),
                    Environment.NewLine,
                    string.Join(", ", _mapperIds));
            }

            if (!TryGetTestsToRun(args, out var testsToRun))
            {
                Console.WriteLine(
                    "Invalid test(s) specified: {0}{1}Available tests: {2}",
                    string.Join(", ", testsToRun),
                    Environment.NewLine,
                    string.Join(", ", _testIds));
            }

            var useManual = mappersToTest.Contains("man");
            var useAgileMapper = mappersToTest.Contains("ag");
            var useAutoMapper = mappersToTest.Contains("au");
            var useExpressMapper = mappersToTest.Contains("exp");
            var useMapster = mappersToTest.Contains("ma");
            var useValueInjecter = mappersToTest.Contains("vi");

            var mapperTestSets = new[]
            {
                new IObjectMapperTest[]
                {
                    useManual ? new ManualCtorMapper() : null,
                    useAgileMapper ? new AgileMapperCtorMapper() : null,
                    useAutoMapper ? new AutoMapperCtorMapper() : null,
                    useExpressMapper ? new ExpressMapperCtorMapper() : null,
                    useMapster ? new MapsterCtorMapper() : null,
                    useValueInjecter ? new ValueInjecterCtorMapper() : null
                },
                new IObjectMapperTest[]
                {
                    useManual ? new ManualComplexTypeMapper() : null,
                    useAgileMapper ? new AgileMapperComplexTypeMapper() : null,
                    useAutoMapper ? new AutoMapperComplexTypeMapper() : null,
                    useExpressMapper ? new ExpressMapperComplexTypeMapper() : null,
                    useMapster ? new MapsterComplexTypeMapper() : null,
                    useValueInjecter ? new ValueInjecterComplexTypeMapper() : null
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperComplexTypeMapperSetup() : null,
                    useAutoMapper ? new AutoMapperComplexTypeMapperSetup() : null,
                    useExpressMapper ? new ExpressMapperComplexTypeMapperSetup() : null,
                    useMapster ? new MapsterComplexTypeMapperSetup() : null
                },
                new IObjectMapperTest[]
                {
                    useManual ? new ManualFlatteningMapper() : null,
                    useAgileMapper ? new AgileMapperFlatteningMapper() : null,
                    useAutoMapper ? new AutoMapperFlatteningMapper() : null,
                    useExpressMapper ? new ExpressMapperFlatteningMapper() : null,
                    useMapster ? new MapsterFlatteningMapper() : null,
                    useValueInjecter ? new ValueInjecterFlatteningMapper() : null
                },
                new IObjectMapperTest[]
                {
                    useManual ? new ManualUnflatteningMapper() : null,
                    useAgileMapper ? new AgileMapperUnflatteningMapper() : null,
                    useAutoMapper ? new AutoMapperUnflatteningMapper(): null,
                    //new ExpressMapperUnflatteningMapper(), // Not supported, NullReferenceException
                    //new MapsterUnflatteningMapper(),       // Not supported, complex type members unpopulated
                    useValueInjecter ? new ValueInjecterUnflatteningMapper() : null
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperUnflatteningMapperSetup() : null,
                    useAutoMapper ? new AutoMapperUnflatteningMapperSetup() : null
                },
                new IObjectMapperTest[]
                {
                    useManual ? new ManualDeepMapper() : null,
                    useAgileMapper ? new AgileMapperDeepMapper() : null,
                    useAutoMapper ? new AutoMapperDeepMapper() : null,
                    useExpressMapper ? new ExpressMapperDeepMapper() : null,
                    useMapster ? new MapsterDeepMapper() : null,
                    useValueInjecter ? new ValueInjecterDeepMapper() : null
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperDeepMapperSetup() : null,
                    useAutoMapper ? new AutoMapperDeepMapperSetup() : null,
                    useExpressMapper ? new ExpressMapperDeepMapperSetup() : null,
                    useMapster ? new MapsterDeepMapperSetup() : null
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperEntityMapper() : null,
                    //useAutoMapper ? new AutoMapperEntityMapper() : null, // Not supported, StackOverflow exception
                    //useMapster ? new MapsterEntityMapper() : null, // Not supported, StackOverflow exception
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperEntityMapperSetup() : null,
                    useAutoMapper ? new AutoMapperEntityMapperSetup() : null,
                    useMapster ? new MapsterEntityMapperSetup() : null
                },
                new IObjectMapperTest[]
                {
                    useAgileMapper ? new AgileMapperInstantiation() : null,
                    useAutoMapper ? new AutoMapperInstantiation() : null,
                    useExpressMapper ? new ExpressMapperInstantiation() : null,
                    useMapster ? new MapsterMapperInstantiation() : null
                }
            };

            for (var i = 0; i < mapperTestSets.Length; i++)
            {
                if (!testsToRun.Contains(_testIds[i]))
                {
                    continue;
                }

                var relevantMapperTests = (mappersToTest == _mapperIds)
                    ? mapperTestSets[i]
                    : mapperTestSets[i].Where(mapper => mapper != null).ToArray();

                MapperTester.Test(relevantMapperTests);
                Console.WriteLine();
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static bool TryGetMappersToTest(IList<string> args, out string[] mappersToTest)
        {
            if (args.Count == 0)
            {
                mappersToTest = _mapperIds;
                return true;
            }

            if (args[0] == "*")
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

        private static bool TryGetTestsToRun(IList<string> args, out string[] testsToRun)
        {
            if (args.Count < 2)
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
