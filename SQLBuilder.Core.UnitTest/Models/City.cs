using SQLBuilder.Core.Attributes;
using System.Data;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_City")]
    public class City
    {
        public int Id { get; set; }
        public int CountryId { get; set; }

        [Column("City_Name"), DataType(IsDbType = true, DbType = DbType.AnsiString)]
        public string CityName { get; set; }
    }

    [Table("Base_City2")]
    public class City2 : City
    {
        public int Age { get; set; }
    }

    [Table("Base_City3")]
    public class City3 : City2
    {
        public string Address { get; set; }
    }
}
