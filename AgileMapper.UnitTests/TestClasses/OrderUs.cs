namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using Common.TestClasses;

    public class OrderUs
    {
        public string Id { get; set; }

        public DateTime DateCreated { get; set; }

        public PaymentTypeUs PaymentType { get; set; }
    }
}