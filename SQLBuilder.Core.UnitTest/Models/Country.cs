using SQLBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Country")]
    public class Country
    {
        [Column("Country_Id")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
