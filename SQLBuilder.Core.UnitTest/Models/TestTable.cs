using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Test", Format = true)]
    public class TestTable
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(Format = true)]
        public string Group { get; set; }

        [Column(Format = true)]
        public string Order { get; set; }

        public string OtherName { get; set; }
    }
}
