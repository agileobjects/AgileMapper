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

            var useManual = mappersToTest.Contains("man");
            var useAgileMapper = mappersToTest.Contains("ag");
            var useAutoMapper = mappersToTest.Contains("au");
            var useExpressMapper = mappersToTest.Contains("exp");
            var useMapster = mappersToTest.Contains("ma");
            var useValueInjecter = mappersToTest.Contains("vi");

            var mapperSets = new[]
            {
                new IObjectMapper[]
                {
                    useManual ? new ManualCtorMapper() : null,
                    useAgileMapper ? new AgileMapperCtorMapper() : null,
                    useAutoMapper ? new AutoMapperCtorMapper() : null,
                    useExpressMapper ? new ExpressMapperCtorMapper() : null,
                    useMapster ? new MapsterCtorMapper() : null,
                    useValueInjecter ? new ValueInjecterCtorMapper() : null
                },
                new IObjectMapper[]
                {
                    useManual? new ManualComplexTypeMapper() : null,
                    useAgileMapper? new AgileMapperComplexTypeMapper() : null,
                    useAutoMapper? new AutoMapperComplexTypeMapper() : null,
                    useExpressMapper? new ExpressMapperComplexTypeMapper() : null,
                    useMapster ? new MapsterComplexTypeMapper() : null,
                    useValueInjecter ? new ValueInjecterComplexTypeMapper() : null
                },
                new IObjectMapper[]
                {
                    useManual? new ManualFlatteningMapper() : null,
                    useAgileMapper? new AgileMapperFlatteningMapper() : null,
                    useAutoMapper? new AutoMapperFlatteningMapper() : null,
                    useExpressMapper? new ExpressMapperFlatteningMapper() : null,
                    useMapster? new MapsterFlatteningMapper() : null,
                    useValueInjecter ? new ValueInjecterFlatteningMapper() : null
                },
                new IObjectMapper[]
                {
                    useManual? new ManualUnflatteningMapper() : null,
                    useAgileMapper? new AgileMapperUnflatteningMapper() : null,
                    //new AutoMapperUnflatteningMapper(),    // Not supported
                    //new ExpressMapperUnflatteningMapper(), // Not supported
                    //new MapsterUnflatteningMapper(),       // Not supported
                    useValueInjecter ? new ValueInjecterUnflatteningMapper() : null
                },
                new IObjectMapper[]
                {
                    useManual? new ManualDeepMapper() : null,
                    useAgileMapper? new AgileMapperDeepMapper() : null,
                    useAutoMapper? new AutoMapperDeepMapper() : null,
                    useExpressMapper? new ExpressMapperDeepMapper() : null,
                    useMapster? new MapsterDeepMapper() : null,
                    useValueInjecter ? new ValueInjecterDeepMapper() : null
                }
            };

            for (var i = 0; i < mapperSets.Length; i++)
            {
                if (!testsToRun.Contains(_testIds[i]))
                {
                    continue;
                }

                var relevantMappers = (mappersToTest == _mapperIds)
                    ? mapperSets[i]
                    : mapperSets[i].Where(mapper => mapper != null).ToArray();

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
