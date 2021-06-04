using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest.Models
{
    [Table("Base_Teacher")]
    public class Teacher
    {
        public string Name { get; set; }

        public int? Age { get; set; }

        public TeacherType? Type { get; set; }
    }

    public enum TeacherType : int
    {
        A,
        B
    }
}
