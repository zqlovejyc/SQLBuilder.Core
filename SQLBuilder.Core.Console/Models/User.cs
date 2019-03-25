using SQLBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKey = System.ComponentModel.DataAnnotations.KeyAttribute;
using SysTable = System.ComponentModel.DataAnnotations.Schema.TableAttribute;
using SysColumn = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;
using CusKey = SQLBuilder.Core.KeyAttribute;

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
