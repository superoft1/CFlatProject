using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class WeldMinDistance
  {
    [Column(IsPrimaryKey = true)] public int NPS { get; set; }
    [Column(IsPrimaryKey = true)] public int EndPrep { get; set; }
    [Column] public double Distance { get; set; }
  }
}