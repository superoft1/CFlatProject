using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class PipingClass
  {
    [Column(IsPrimaryKey = true)] public int ID { get; set; }
    [Column] public string Name { get; set; }
    [Column] public int Rating { get; set; }
    [Column] public double CA { get; set; }
    [Column] public int Standard { get; set; }
    [Column(CanBeNull = true)] public string ServiceDescription { get; set; }
  }
}
