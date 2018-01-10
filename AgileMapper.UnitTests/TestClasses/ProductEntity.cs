namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;

    internal class ProductEntity : EntityBase
    {
        public Guid ProductSku
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }
    }
}