namespace Chiyoda.DB
{
  public class RecordOfCatalog :RecordBase
  {
    internal int ID { get; set; }
    public string Standard { get; internal set; }
    public string ShortCode { get; internal set; }
    public string IdentCode { get; internal set; }
  }
}
