using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class DimensionOfPipeGuide
  {
    [Column(IsPrimaryKey = true)]
    public int NPS { get; set; }

    [Column(IsPrimaryKey = true)]
    public string GuideType { get; set; }

    [Column]
    public double Height { get; set; }

    [Column]
    public double Width { get; set; }
  }
}