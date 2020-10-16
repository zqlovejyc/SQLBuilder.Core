using CusKey = SQLBuilder.Core.Attributes.KeyAttribute;
using SysTable = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace SQLBuilder.Core
{
    [SysTable("Base_UserInfo", Schema = "dbo")]
    public class UserInfo
    {
        [CusKey(name: "userId")]
        public int? Id { get; set; }
        [CusKey]
        public int Sex { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
