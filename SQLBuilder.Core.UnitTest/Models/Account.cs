using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Account")]
    public class Account
    {
        //[Key]
        //[Column(Insert = false)]

        [Key(Identity = true)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
