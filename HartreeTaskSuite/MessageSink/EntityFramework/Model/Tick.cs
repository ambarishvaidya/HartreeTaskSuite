using System.ComponentModel.DataAnnotations.Schema;

namespace MessageSink.EntityFramework.Model
{
    [Table("DataDump")]
    public class Tick
    {
        [Column("key", TypeName = "nvarchar(20)")]
        public string TickKey { get; set; }

        [Column("time", TypeName = "datetime")]
        public DateTime TickTimeInUtc { get; set; }

        [Column("value", TypeName = "float")]
        public double TickValue { get; set; }

        public override string ToString()
        {
            return $"{TickKey} {TickTimeInUtc} {TickValue}";
        }
    }
}
