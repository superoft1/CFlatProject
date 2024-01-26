namespace Chiyoda.DB
{
  /// <summary>
  /// 浮動小数点同士の一致条件を定義
  /// </summary>
  public class Tolerance
  {
    public static double NPS => 0.001; // 0.125 is the smallest value of NPS ... 0.124 - 0.126 is allowable

    public static double AssertEqualDelta => 1e-2;
  }
}