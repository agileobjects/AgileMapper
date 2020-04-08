namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using Common.TestClasses;

    public class OrderUsViewModel
    {
        public DateTime DatePlaced { get; set; }

        public PaymentTypeUs PaymentType { get; set; }

        public bool FirstItemHasProductName { get; set; }
    }
}