﻿namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;
    using Common.TestClasses;

    public class PublicNullableTitle
    {
        [Key]
        public int Id { get; set; }

        public Title? Value { get; set; }
    }
}