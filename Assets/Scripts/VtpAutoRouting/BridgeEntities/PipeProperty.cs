namespace VtpAutoRouting
{
  /// <summary>
  /// 直径を表現するクラス
  /// Chiyodev.CAD.Modelにも類似のクラスがあるが
  /// 将来的なモジュール分割を考慮して、新規作成
  /// </summary>
  public class PipeProperty : Routing.IPipeProperty
  {
    private readonly Chiyoda.CAD.Model.Diameter _diameter ;
    // 単位はm

    public PipeProperty( Chiyoda.CAD.Model.Diameter diam )
    {
      _diameter = diam ;
    }

    public float Outside => (float)_diameter.OutsideMeter ;
    
    // 単位はmm
    public int NPSmm => _diameter.NpsMm ;
  }
}