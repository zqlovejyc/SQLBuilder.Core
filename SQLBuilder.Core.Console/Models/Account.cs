using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLBuilder;

namespace SQLBuilder.Core
{
    [Table("Base_Account", Schema = "")]
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
