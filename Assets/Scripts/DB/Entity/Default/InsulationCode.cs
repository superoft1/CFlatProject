using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class InsulationCode
  {
    [Column] public string Code { get; set; }
    [Column] public int ThicknessGroupID { get; set; }    
    [Column(CanBeNull = true)] public string Description { get; set; }
  }
}