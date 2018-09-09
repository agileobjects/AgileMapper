namespace AgileObjects.AgileMapper.PerformanceTesting.TestClasses
{
    using System;

    public static class Flattening
    {
        public class ModelObject
        {
            public DateTime BaseDate { get; set; }

            public ModelSubObject Sub { get; set; }

            public ModelSubObject Sub2 { get; set; }

            public ModelSubObject SubWithExtraName { get; set; }
        }

        public class ModelSubObject
        {
            public string ProperName { get; set; }

            public ModelSubSubObject SubSub { get; set; }
        }

        public class ModelSubSubObject
        {
            public string CoolProperty { get; set; }
        }

        public class ModelDto
        {
            public DateTime BaseDate { get; set; }

            public string SubProperName { get; set; }

            public string Sub2ProperName { get; set; }

            public string SubWithExtraNameProperName { get; set; }

            public string SubSubSubCoolProperty { get; set; }
        }
    }
}
