using System;

namespace AgileObjects.AgileMapper.PerformanceTester.TestClasses
{
    public class ModelObject
    {
        public DateTime BaseDate { get; set; }

        public ModelSubObject Sub { get; set; }

        public ModelSubObject Sub2 { get; set; }

        public ModelSubObject SubWithExtraName { get; set; }
    }
}
