using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Plot
{
  [Table]
  public class EquipHazardClearance
  {
    [Column(IsPrimaryKey = true)] public string EquipHazardGroup1 { get; set; }
    [Column(IsPrimaryKey = true)] public string EquipHazardGroup2 { get; set; }    
    [Column] public double Clearance_ft { get; set; }    
    [Column] public double Clearance_m { get; set; }
  }
}