using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfFlange
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfFlangeTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      Assert.AreEqual( 46+2, table.Get(15 ).Height, delta ) ;
      Assert.AreEqual( 51+2, table.Get(20 ).Height, delta ) ;
      Assert.AreEqual( 54+2, table.Get(25 ).Height, delta ) ;
      Assert.AreEqual( 60+2, table.Get(40 ).Height, delta ) ;
      Assert.AreEqual( 62+2, table.Get(50 ).Height, delta ) ;
      Assert.AreEqual( 68+2, table.Get(65 ).Height, delta ) ;
      Assert.AreEqual( 68+2, table.Get(80 ).Height, delta ) ;
      Assert.AreEqual( 75+2, table.Get(100 ).Height, delta ) ;
      Assert.AreEqual( 87+2, table.Get(150 ).Height, delta ) ;
      Assert.AreEqual( 100+2, table.Get(200 ).Height, delta ) ;
      Assert.AreEqual( 100+2, table.Get(250 ).Height, delta ) ;
      Assert.AreEqual( 113+2, table.Get(300 ).Height, delta ) ;
      Assert.AreEqual( 125+2, table.Get(350 ).Height, delta ) ;
      Assert.AreEqual( 125+2, table.Get(400 ).Height, delta ) ;
      Assert.AreEqual( 138+2, table.Get(450 ).Height, delta ) ;
      Assert.AreEqual( 143+2, table.Get(500 ).Height, delta ) ;
      Assert.AreEqual( 151+2, table.Get(600 ).Height, delta ) ;

      {
        var record = table.Get(200, new Rating("ASME", 900), "WN");
        Assert.AreEqual(470, record.O, delta);
        Assert.AreEqual(63.5, record.tf, delta);
        Assert.AreEqual(298, record.X, delta);
        Assert.AreEqual(162, record.Y, delta);
        Assert.AreEqual(7, record.f, delta);
        Assert.AreEqual(162f + 7, record.Height, delta);
      }

      {
        var record = table.Get(65, new Rating("ASME", 1500), "SW");
        Assert.AreEqual(245, record.O, delta);
        Assert.AreEqual(41.3, record.tf, delta);
        Assert.AreEqual(124, record.X, delta);
        Assert.AreEqual(64, record.Y, delta);
        Assert.AreEqual(7, record.f, delta);
        Assert.AreEqual(64f + 7, record.Height, delta);
      }


      // 存在しない組み合わせ
      Assert.Throws<NoRecordFoundException>(() => table.Get(11));
    }
  }
}