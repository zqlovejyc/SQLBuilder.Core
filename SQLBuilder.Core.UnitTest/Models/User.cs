using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_UserInfo")]
    public class UserInfo
    {
        /// <summary>
        /// 主键，且更新实体时，不进行更新
        /// </summary>
        [Key]
        [Column(Update = false)]
        public int? Id { get; set; }
        public int Sex { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Const
    {
        public static string Name = "张三";
    }
}
