namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using AbstractMappers;

    internal class MapperTester
    {
        public static void Test(IEnumerable<IObjectMapper> mappers)
        {
            const int NUMBER_OF_MAPPINGS = 1000000;

            foreach (var mapper in mappers)
            {
                mapper.Initialise();
                mapper.Map();

                var timer = Stopwatch.StartNew();

                for (var i = 0; i < NUMBER_OF_MAPPINGS; i++)
                {
                    mapper.Map();
                }

                timer.Stop();

                Console.WriteLine(mapper.Name.PadRight(40) + timer.Elapsed.TotalSeconds);
            }
        }
    }
}