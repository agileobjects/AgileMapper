namespace AgileObjects.AgileMapper.PerformanceTester.NetCore21
{
    using PerformanceTesting;

    public class Program
    {
        public static void Main(string[] args)
        {
            new PerformanceTestRunner()
                //.AddTest(MapperIds.Mapster, null)
                .Run(args);
        }
    }
}
