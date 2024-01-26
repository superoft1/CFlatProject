using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_Standard
  {
    [Column(IsPrimaryKey = true)]
    public int ID { get; set; }

    [Column]
    public string Name { get; set; }
  }
}