using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class InsulationThickness
  {
    [Column(IsPrimaryKey = true)] public int GroupID { get; set; }
    [Column(IsPrimaryKey = true)] public int NPS { get; set; }
    [Column(IsPrimaryKey = true)] public double Temperature { get; set; }
    [Column] public double Thickness { get; set; }
  }
}