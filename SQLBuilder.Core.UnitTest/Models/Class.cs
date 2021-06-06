using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Class")]
    public class Class
    {
        public int? Id { get; set; }
        [Column(Insert = false)]
        public int CityId { get; set; }
        [Column(Update = false)]
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
