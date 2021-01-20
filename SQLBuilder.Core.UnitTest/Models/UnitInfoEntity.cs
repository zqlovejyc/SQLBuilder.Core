using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.UnitTest
{
    [Table("WF_UNITINFO")]
    public class UnitInfoEntity
    {
        [Column("WORKORDER")]
        public string WorkOrder { get; set; }

        [Column("UNITID")]
        public string UnitId { get; set; }

        [Column("PANELNO")]
        public string PanelNo { get; set; }

        [Column("ENABLED")]
        public int? Enabled { get; set; }
    }

    public class DryBoxInput
    {
        public string BarCodeType { get; set; }

        public string[] BarCodes { get; set; }

        public int Enabled { get; set; }
    }

    public class BarCodeType
    {
        public const string UnitId = "UnitId";

        public const string Panel = "Panel";
    }
}
