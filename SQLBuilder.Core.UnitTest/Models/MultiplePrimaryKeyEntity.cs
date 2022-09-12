using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest.Models
{

    [Table("MultipleKey")]
    public class MultiplePrimaryKeyEntity
    {
        [Key] public int Id { get; set; }
        [Key] public string CompanyId { get; set; }

        public string Address { get; set; }

        public string City { get; set; }
    }
}
