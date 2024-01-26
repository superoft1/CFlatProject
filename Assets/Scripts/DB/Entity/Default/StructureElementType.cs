using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class StructureElementType
  { 
    [Column(IsPrimaryKey = true)] public int ID { get; set; }
    [Column] public string Type { get; set; }
  }
}