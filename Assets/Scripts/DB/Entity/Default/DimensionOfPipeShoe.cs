using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class DimensionOfPipeShoe
  {
    [Column(IsPrimaryKey = true)]
    public int NPS { get; set; }

    [Column(IsPrimaryKey = true)]
    public string ShoeType { get; set; }

    [Column]
    public double CenterToEnd { get; set; }

    [Column]
    public double Width { get; set; }
  }
}