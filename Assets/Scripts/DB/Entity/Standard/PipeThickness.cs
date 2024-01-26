using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_PipeThickness
  {
    [Column(IsPrimaryKey = true)] public int ID { get; set; }
    [Column(IsPrimaryKey = true)] public int NPS { get; set; }
    [Column(CanBeNull = true)] public string IdentificationNote { get; set; }
    [Column(CanBeNull = true)] public int? Schedule_No { get; set; }
    [Column] public double WallThickness_mm { get; set; }
  }
}
