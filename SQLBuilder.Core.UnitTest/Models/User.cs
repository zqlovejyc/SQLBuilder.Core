using SQLBuilder.Core.Attributes;
using System.Data;

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
        [DataType(IsDbType = true, DbType = DbType.Int64)]
        public int? Id { get; set; }
        public int Sex { get; set; }

        [DataType(IsDbType = true, DbType = DbType.String)]
        public string Name { get; set; }

        [DataType(IsDbType = true, DbType = DbType.AnsiStringFixedLength, IsFixedLength = true, FixedLength = 20)]
        public string Email { get; set; }
    }

    /// <summary>
    /// 查询转换实体
    /// </summary>
    public class UserDto
    {
        [Column("SEX", Format = true)]
        public int Sex { get; set; }
        public string Name { get; set; }
    }

    public class Const
    {
        public static string Name = "张三";
    }
}
