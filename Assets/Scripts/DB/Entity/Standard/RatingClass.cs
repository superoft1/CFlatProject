using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_RatingClass
  {
    [Column(IsPrimaryKey = true)] public int ID       { get; set; }
    [Column]                      public int Standard { get; set; }
    [Column]                      public int Rating   { get; set; }
  }
}
