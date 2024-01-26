using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfCaps
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfCapsTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      //正常系 最小径抽出
      {
        var records = table.GetOne(15);
        Assert.AreEqual(25, records.Length, delta);
      }

      //正常系 最大径抽出
      {
        var records = table.GetOne(1200);
        Assert.AreEqual(343, records.Length, delta);
      }

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0));
    }
  }
}