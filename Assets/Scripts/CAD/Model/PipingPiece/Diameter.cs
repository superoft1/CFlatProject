using Chiyoda.DB ;

namespace Chiyoda.CAD.Model
{
  //現状維持のためにところどころ暫定処理が入る
  //後にDiameterLevel関連箇所を消去する
  public class Diameter
  {
    /// <summary>
    /// 外形(ミリメートル)
    /// </summary>
    private double OutsideMm => DiameterFactory.GetOutsideDiameterMm(this);

    public int NpsMm { get ; }
    public double NpsInch { get ; }

    /// <summary>
    /// 外形(メートル)
    /// </summary>
    public double OutsideMeter => OutsideMm.Millimeters() ;

    /// <summary>
    /// 最小溶接間距離
    /// </summary>
    /// <returns></returns>
    public double WeldMinDistance()
    {
      var table = Chiyoda.DB.DB.Get<WeldMinDistanceTable>();
      return table.GetDistance( (float)NpsMm ) ;
    }

    public Diameter(int npsMm, double npsInch)
    {
      NpsMm = npsMm;
      NpsInch = npsInch;
    }

    public static Diameter Default()
    {
      return new Diameter(0, 0) ;
    }
  }
}
