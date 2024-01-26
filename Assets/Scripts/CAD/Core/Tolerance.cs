namespace Chiyoda.CAD.Core
{
  public class Tolerance
  {    
    /// <summary>
    /// 本来、インポートポスト処理で使うためのトレランス
    /// 汎用的な配管ファイルに入っている座標は、1e-3以上ずれていることが多い
    /// [TODO] インポートポスト処理でずれを修正してやる必要がある
    /// 自動ルーティング等ではこのトレランスを使わないこと
    /// </summary>
    public const double MergeToleranceForImporter = 1e-2;

    public const double DistanceTolerance = 1e-5;

    public const double AngleTolerance = 1e-2;

    public const double DoubleEpsilon = 1e-12;

    public const float FloatEpsilon = 1e-5f;
  }
}