using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_DimensionOfFlange
  {
    [Column(IsPrimaryKey = true)]
    public int NPS { get; set; }

    [Column(IsPrimaryKey = true)]
    public int Standard { get; set; }
    
    [Column(IsPrimaryKey = true)]
    public int NP { get; set; }

    [Column]
    public double O { get; set; }

    [Column]
    public double tf { get; set; }

    [Column]
    public double X { get; set; }

  }
}