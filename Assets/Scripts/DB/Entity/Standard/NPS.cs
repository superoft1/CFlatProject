using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Standard
{
  [Table]
  public class STD_NPS
  {
    [Column(IsPrimaryKey = true)]
    public int ID { get; set; }

    [Column]
    public int mm { get; set; }

    [Column]
    public double Inchi { get; set; }

    [Column]
    public string InchiDisp { get; set; }
  }
}