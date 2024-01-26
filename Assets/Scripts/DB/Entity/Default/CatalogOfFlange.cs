using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class CatalogOfFlange
  {
    [Column(IsPrimaryKey = true)] public int ID { get; set; }
    [Column(IsPrimaryKey = true)] public string ShortCode { get; set; }
    [Column] public int Standard { get; set; }
    [Column(CanBeNull = true)] public string IdentCode { get; set; }
    [Column] public int NPS { get; set; }
    [Column] public int Rating { get; set; }
    [Column] public int EndPrep { get; set; }
    [Column] public int FlangeFace { get; set; }
  }
}
