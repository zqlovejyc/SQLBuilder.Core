using SQLBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Student")]
    public class Student
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
    }
}
