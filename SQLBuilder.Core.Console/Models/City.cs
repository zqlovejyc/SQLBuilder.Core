using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core
{
    [Table("Base_City")]
    public class City
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string CityName { get; set; }
    }
}
