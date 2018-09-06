// AgileMapper and AutoMapper perform better with this attribute applied,
// but ExpressMapper and Mapster throw exceptions - uncomment to test the
// Agile and Auto at their fastest:
//[assembly: System.Security.AllowPartiallyTrustedCallers]

namespace AgileObjects.AgileMapper.PerformanceTester.Net45
{
    using ConcreteMappers.ExpressMapper;
    using PerformanceTesting;

    // Specify comma-separated sets of mapper Ids and (optionally) test Ids from the string arrays
    // below if desired. e.g:
    //  - Run the deep and complex tests for AgileMapper:
    //    ag deep,compl
    //  - Run the constructor and flattening mapping tests for the manual and ExpressMapper mappers:
    //    man,exp ctor,flat
    public class Program
    {
        public static void Main(string[] args)
        {
            new PerformanceTestRunner()
                .AddMapper("exp", typeof(ExpressMapperCtorMapper))
                .Run(args);
        }
    }
}
