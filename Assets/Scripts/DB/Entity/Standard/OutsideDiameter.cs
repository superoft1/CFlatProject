using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_OutsideDiameter
  {
    [Column(IsPrimaryKey = true)] public int Standard { get; set; }
    [Column(IsPrimaryKey = true)] public int NPS { get; set; }
    [Column] public double mm { get; set; }
  }
}
