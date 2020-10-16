using SQLBuilder.Core.Attributes;

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
