using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_TypeOfEndPrep
  {
    [Column(IsPrimaryKey = true)]
    public int ID { get; set; }

    [Column]
    public string Code { get; set; }

    [Column]
    public string Name { get; set; }
  }
}
