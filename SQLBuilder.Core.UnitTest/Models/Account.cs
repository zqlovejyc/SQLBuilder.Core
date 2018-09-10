using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLBuilder.Core;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Account")]
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
