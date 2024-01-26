using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class WeldMinDistance
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetCode()
    {
      var table = Chiyoda.DB.DB.Get<WeldMinDistanceTable>();
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      // NPS 1mm -> WeldMin 100mm
      Assert.AreEqual( 100.0, table.GetDistance(6.0), delta );
      Assert.AreEqual( 100.0, table.GetDistance(150.0), delta );
      Assert.AreEqual( 150.0, table.GetDistance(200.0), delta );
      Assert.AreEqual( 150.0, table.GetDistance(400.0), delta );
      Assert.AreEqual( 200.0, table.GetDistance(450.0), delta );
      Assert.AreEqual( 200.0, table.GetDistance(2000.0), delta );
       
      // WNとSWの場合の比較
      Assert.AreEqual( 100.0, table.GetDistance(6.0, "BW"), delta );
      Assert.AreEqual( 75.0, table.GetDistance(6.0, "SW"), delta );

      // 存在しないNPS => exception
      Assert.Throws<NoRecordFoundException>(() => table.GetDistance(0.0f));
      Assert.Throws<NoRecordFoundException>(() => table.GetDistance(2050.0f));

    }
  }
}