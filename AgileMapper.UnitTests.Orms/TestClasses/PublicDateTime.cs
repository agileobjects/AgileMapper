﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PublicDateTime
    {
        [Key]
        public int Id { get; set; }


        public DateTime Value { get; set; }
    }
}