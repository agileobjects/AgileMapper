namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Common.TestClasses;

    public class OrderUk
    {
        [Key]
        public int Id { get; set; }

        public DateTime DatePlaced { get; set; }

        public PaymentTypeUk PaymentType { get; set; }

        public IList<OrderItem> Items { get; set; }
    }
}