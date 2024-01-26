using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_DimensionOfElbows
  {
    [Column(IsPrimaryKey = true)]
    public int Standard { get; set; }

    [Column(IsPrimaryKey = true)]
    public int NPS { get; set; }

    [Column(IsPrimaryKey = true)]
    public int ElbowType { get; set; }

    [Column]
    public double CenterToEnd { get; set; }
  }
}