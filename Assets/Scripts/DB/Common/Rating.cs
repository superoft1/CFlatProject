namespace Chiyoda.DB
{
  public class Rating
  {
    public string Standard { get; set; }

    public int NP { get; set; }

    public Rating()
    {
      this.Standard = "ASME";
      this.NP = 150;
    }
    
    public Rating(string Standard , int NP)
    {
      this.Standard = Standard;
      this.NP = NP;
    }
    
  }
}