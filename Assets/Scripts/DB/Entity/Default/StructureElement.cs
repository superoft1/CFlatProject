using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class StructureElement
  {
    [Column(IsPrimaryKey = true, CanBeNull = false)] public int Spec { get; set; }
    [Column(IsPrimaryKey = true, CanBeNull = false)] public int Type { get; set; }
    [Column(IsPrimaryKey = true, CanBeNull = false)] public string Standard { get; set; }
    [Column(CanBeNull = true)] public double? H_D { get; set; }
    [Column(CanBeNull = true)] public double? B { get; set; }	
    [Column(CanBeNull = true)] public double? tw_t	{ get; set; }
    [Column(CanBeNull = true)] public double? tf	{ get; set; }
    [Column(CanBeNull = true)] public double? AX	{ get; set; }
    [Column(CanBeNull = true)] public double? Ix	{ get; set; }
    [Column(CanBeNull = true)] public double? Sx	{ get; set; }
    [Column(CanBeNull = true)] public double? Sy	{ get; set; }
    [Column(CanBeNull = true)] public double? Rx	{ get; set; }
    [Column(CanBeNull = true)] public double? Ry	{ get; set; }
    [Column(CanBeNull = true)] public double? Weight { get; set; }
  }
}