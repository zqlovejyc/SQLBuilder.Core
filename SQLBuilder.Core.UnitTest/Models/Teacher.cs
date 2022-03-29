using SQLBuilder.Core.Attributes;
using System.Data;

namespace SQLBuilder.Core.UnitTest
{
    [Table("Base_Teacher")]
    public class Teacher
    {
        public string Name { get; set; }

        public int? Age { get; set; }

        [DataType(IsDbType = true, DbType = DbType.Int32)]
        public int ClassId { get; set; }

        public TeacherType? Type { get; set; }
    }

    public enum TeacherType : int
    {
        A = 1,
        B = 2
    }

    public class TeacherResponse
    {
        public string TeacherName { get; set; }
        public string ClassName { get; set; }
    }
}
