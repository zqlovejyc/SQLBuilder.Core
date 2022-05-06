using SQLBuilder.Core.Attributes;
using System;
using System.Data;

namespace SQLBuilder.Core.UnitTest.Models
{
    [Table("Base_Log")]
    public class Log
    {
        [Key, DataType(IsDbType = true, DbType = DbType.Guid)]
        public Guid Id { get; set; }

        [DataType(IsDbType = true, DbType = DbType.AnsiStringFixedLength)]
        public string User { get; set; }
        public string Message { get; set; }
    }
}
