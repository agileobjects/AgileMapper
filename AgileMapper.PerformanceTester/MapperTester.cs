namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using AbstractMappers;

    internal class MapperTester
    {
        public static void Test(IEnumerable<IObjectMapperTest> mapperTests)
        {
            foreach (var test in mapperTests)
            {
                test.Initialise();

                var result = test.Execute(Stopwatch.StartNew());

                test.Verify(result);

                var timer = Stopwatch.StartNew();

                for (var i = 0; i < test.NumberOfExecutions; i++)
                {
                    test.Execute(timer);
                }

                timer.Stop();

                Console.WriteLine(test.Name.PadRight(40) + timer.Elapsed.TotalSeconds);
            }
        }
    }
}