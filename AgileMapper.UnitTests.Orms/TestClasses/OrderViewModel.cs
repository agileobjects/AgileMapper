namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using UnitTests.TestClasses;

    public class OrderViewModel
    {
        public DateTime DatePlaced { get; set; }

        public PaymentTypeUs PaymentType { get; set; }
    }
}