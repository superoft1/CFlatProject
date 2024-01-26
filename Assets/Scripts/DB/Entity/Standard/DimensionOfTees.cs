using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_DimensionOfTees
  {
    [Column(IsPrimaryKey = true)]
    public int Standard { get; set; }

    [Column(IsPrimaryKey = true)]
    public int NPS_H { get; set; }

    [Column(IsPrimaryKey = true)]
    public int NPS_B { get; set; }

    [Column]
    public double CenterToEnd_Run_C { get; set; }

    [Column]
    public double CenterToEnd_Outlet_M { get; set; }
  }
}
