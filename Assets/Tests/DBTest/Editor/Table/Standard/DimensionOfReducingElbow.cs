using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfReducingElbow
    {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfReducingElbowTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      //正常系 1行目抽出
      {
        var records = table.GetOne(50, 40);
        Assert.AreEqual(76, records.Radius, delta);
      }

      //正常系 最終行抽出
      {
        var records = table.GetOne(600, 300);
        Assert.AreEqual(914, records.Radius, delta);
      }

      //異常系 不可能な組み合わせの場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(50, 600));
    }
  }
}