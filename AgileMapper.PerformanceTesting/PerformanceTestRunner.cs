// Largely based on the AutoMapper benchmark project:
// Project: https://github.com/AutoMapper/AutoMapper/tree/master/src/Benchmark
// Licence: https://github.com/AutoMapper/AutoMapper/blob/master/LICENSE.txt

namespace AgileObjects.AgileMapper.PerformanceTesting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using ConcreteMappers.AgileMapper;
    using ConcreteMappers.AutoMapper;
    using ConcreteMappers.Manual;
    using ConcreteMappers.Mapster;
    using ConcreteMappers.ValueInjecter;
    using Extensions;
    using static MapperIds;

    public class PerformanceTestRunner
    {
        private static readonly string[] _testIds =
            { "ctor", "compl", "compls", "flat", "unflat", "unflats", "deep", "deeps", "ent", "ents", "new" };

        private readonly Dictionary<string, ICollection<IObjectMapperTest>> _mapperTestsByMapperId;

        public PerformanceTestRunner()
        {
            _mapperTestsByMapperId = new Dictionary<string, ICollection<IObjectMapperTest>>
            {
                [Manual] = GetMapperTestsFor(typeof(ManualCtorMapper)),
                [AgileMapper] = GetMapperTestsFor(typeof(AgileMapperCtorMapper)),
                [AutoMapper] = GetMapperTestsFor(typeof(AutoMapperCtorMapper)),
                [Mapster] = GetMapperTestsFor(typeof(MapsterCtorMapper)),
                [ValueInjecter] = GetMapperTestsFor(typeof(ValueInjecterCtorMapper))
            };
        }

        private static ICollection<IObjectMapperTest> GetMapperTestsFor(Type exampleMapperTestType)
        {
            return exampleMapperTestType
                .Assembly
                .GetTypes()
                .Filter(t => (t.Namespace == exampleMapperTestType.Namespace) && typeof(IObjectMapperTest).IsAssignableFrom(t))
                .Project(CreateTest)
                .ToList();
        }

        private static IObjectMapperTest CreateTest(Type testType)
            => (IObjectMapperTest)Activator.CreateInstance(testType);

        public PerformanceTestRunner AddMapper(string id, Type exampleMapperTestType)
        {
            _mapperTestsByMapperId[id] = GetMapperTestsFor(exampleMapperTestType);
            return this;
        }

        public PerformanceTestRunner AddTest(string mapperId, Type testType)
        {
            _mapperTestsByMapperId[mapperId].Add(CreateTest(testType));
            return this;
        }

        public void Run(string[] args)
        {
            Console.WriteLine("Starting...");

            if (!TryGetMappersToTest(args, out var mappersToTest))
            {
                Console.WriteLine(
                    "Invalid mapper(s) specified: {0}{1}Available mapper ids: {2}",
                    string.Join(", ", mappersToTest),
                    Environment.NewLine,
                    string.Join(", ", _mapperTestsByMapperId.Keys));
            }

            if (!TryGetTestsToRun(args, out var testsToRun))
            {
                Console.WriteLine(
                    "Invalid test(s) specified: {0}{1}Available tests: {2}",
                    string.Join(", ", testsToRun),
                    Environment.NewLine,
                    string.Join(", ", _testIds));
            }

            var mapperTestsByType = _mapperTestsByMapperId
                .SelectMany(kvp => kvp.Value.Project(mapperTest => new
                {
                    MapperId = kvp.Key,
                    Test = mapperTest
                }))
                .GroupBy(d => d.Test.Type)
                .ToDictionary(grp => grp.Key, grp => grp.ToArray());

            var mapperTestSets = _testIds
                .Filter(testId => testsToRun.Contains(testId))
                .Project(testId => mapperTestsByType[testId]
                    .Filter(d => mappersToTest.Contains(d.MapperId))
                    .Project(d => d.Test)
                    .ToArray())
                .ToArray();

            foreach (var mapperTestSet in mapperTestSets)
            {
                MapperTester.Test(mapperTestSet);
                Console.WriteLine();
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private bool TryGetMappersToTest(IList<string> args, out ICollection<string> mappersToTest)
        {
            if (args.Count == 0)
            {
                mappersToTest = _mapperTestsByMapperId.Keys;
                return true;
            }

            if (args[0] == "*")
            {
                mappersToTest = _mapperTestsByMapperId.Keys;
                return true;
            }

            var mapperIds = args[0].ToLowerInvariant().Split(',');
            var invalidMapperIds = mapperIds.Except(_mapperTestsByMapperId.Keys).ToArray();

            if (invalidMapperIds.Any())
            {
                mappersToTest = invalidMapperIds;
                return false;
            }

            mappersToTest = _mapperTestsByMapperId.Keys.Intersect(mapperIds).ToArray();
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
