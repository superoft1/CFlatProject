using Routing.Process;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.Routing
{
  public class PipeClearanceTest
  {
    [Test]
    public void NoInsulation()
    {
      var delta = 1e-4f ;
      // 2つのパイプのNPS,InsulationCode,Temperatureからクリアランスを計算する
      Assert.AreEqual( 0.01f, PipeClearanceCalculator.GetClearanceBetween( null, null ), delta ) ;
    }

  }
}