using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBuilder;

namespace SQLBuilder.Core
{
    [Table("student")]
    public class MyStudent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public decimal? Score { get; set; }
    }
}
