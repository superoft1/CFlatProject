using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class PipingClassStock
  {
    [Column(IsPrimaryKey = true)] public int PipingClass { get; set; }
    [Column(IsPrimaryKey = true)] public int Catalog { get; set; }
    [Column(IsPrimaryKey = true)] public string ShortCode { get; set; }
  }
}
