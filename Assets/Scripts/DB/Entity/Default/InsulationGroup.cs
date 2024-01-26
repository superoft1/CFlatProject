using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class InsulationGroup
  {
    [Column(IsPrimaryKey = true)] public int    ID          { get; set; }
    [Column]                      public string Description { get; set; }
  }
}