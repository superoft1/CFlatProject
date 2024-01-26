using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_PTRating
  {
    [Column(IsPrimaryKey = true)]
    public string MaterialGroup { get; set; }

    [Column(IsPrimaryKey = true)]
    public double Temperature { get; set; }

    [Column(IsPrimaryKey = true)]
    public int Class { get; set; }

    [Column]
    public double WorkingPressure { get; set; }
  }
}