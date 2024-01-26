using System.Data.Linq.Mapping;

namespace Chiyoda.DB.Entity.Default
{
  [Table]
  public class CatalogOfTee
  {
    [Column(IsPrimaryKey = true)]    public int     ID              { get; set; }
    [Column(IsPrimaryKey = true)]    public string  ShortCode       { get; set; }
    [Column(CanBeNull = false)]      public int     Standard        { get; set; }
    [Column(CanBeNull = false)]      public int     PipeThickness   { get; set; }
    [Column(CanBeNull = false)]      public int     NPS_B           { get; set; }
    [Column(CanBeNull = false)]      public int     EndPrep         { get; set; }
    [Column(CanBeNull = true)]       public string  IdentCode       { get; set; }
  }
}
