using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_DimensionOfReducingElbow
  {
    [Column(IsPrimaryKey = true)]
    public int Standard { get; set; }

    [Column(IsPrimaryKey = true)]
    public int NPS_L { get; set; }

    [Column(IsPrimaryKey = true)]
    public int NPS_S { get; set; }

    [Column]
    public double CenterToEnd { get; set; }
  }
}