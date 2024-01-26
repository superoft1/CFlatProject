using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfReducer
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfReducerTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      // 1回で径を落とせるケース
      {
        var records = table.Get(20, 15);
        Assert.AreEqual(1, records.Count());
        Assert.AreEqual(38, records.First().Height, delta);
      }

      // 2回で径を落とすケース
      {
        var records = table.Get(400, 150);
        Assert.AreEqual(2, records.Count());

        // 16x8
        Assert.AreEqual(400, records[0].NPS_L_mm);
        Assert.AreEqual(200, records[0].NPS_S_mm);
        Assert.AreEqual(356, records[0].Height, delta);

        // 8x6
        Assert.AreEqual(200, records[1].NPS_L_mm);
        Assert.AreEqual(150, records[1].NPS_S_mm);
        Assert.AreEqual(152, records[1].Height, delta);
      }

      // TODO 3回で径を落とすケース
      {
        // 24 -> 5
        var records = table.Get(600, 125);
        Assert.AreEqual(3, records.Count());

        // 24x16
        Assert.AreEqual(600, records[0].NPS_L_mm);
        Assert.AreEqual(400, records[0].NPS_S_mm);
        Assert.AreEqual(508.0, records[0].Height, delta);

        // 16x8
        Assert.AreEqual(400, records[1].NPS_L_mm);
        Assert.AreEqual(200, records[1].NPS_S_mm);
        Assert.AreEqual(356.0, records[1].Height, delta);

        // 8x5
        Assert.AreEqual(200, records[2].NPS_L_mm);
        Assert.AreEqual(125, records[2].NPS_S_mm);
        Assert.AreEqual(152.0, records[2].Height, delta);
      }

      // 最大NPSから最小NPSへ径を落とす(8回)
      {
        // 36 -> 3
        var records = table.Get(1200, 10);

        // 36x32
        Assert.AreEqual(1200, records[0].NPS_L_mm);
        Assert.AreEqual(1000, records[0].NPS_S_mm);
        Assert.AreEqual(711.0, records[0].Height, delta);

        // 32x27
        Assert.AreEqual(1000, records[1].NPS_L_mm);
        Assert.AreEqual(750, records[1].NPS_S_mm);
        Assert.AreEqual(610.0, records[1].Height, delta);

        // 27x22
        Assert.AreEqual(750, records[2].NPS_L_mm);
        Assert.AreEqual(500, records[2].NPS_S_mm);
        Assert.AreEqual(610.0, records[2].Height, delta);

        // 22x18
        Assert.AreEqual(500, records[3].NPS_L_mm);
        Assert.AreEqual(300, records[3].NPS_S_mm);
        Assert.AreEqual(508.0, records[3].Height, delta);

        // 18x14
        Assert.AreEqual(300, records[4].NPS_L_mm);
        Assert.AreEqual(125, records[4].NPS_S_mm);
        Assert.AreEqual(203.0, records[4].Height, delta);

        // 14x9
        Assert.AreEqual(125, records[5].NPS_L_mm);
        Assert.AreEqual(50, records[5].NPS_S_mm);
        Assert.AreEqual(127.0, records[5].Height, delta);

        // 9x5
        Assert.AreEqual(50, records[6].NPS_L_mm);
        Assert.AreEqual(20, records[6].NPS_S_mm);
        Assert.AreEqual(76.0, records[6].Height, delta);

        // 5x3
        Assert.AreEqual(20, records[7].NPS_L_mm);
        Assert.AreEqual(10, records[7].NPS_S_mm);
        Assert.AreEqual(38.0, records[7].Height, delta);
      }

      // 不可能な組み合わせの場合、NoRecordFoundExceptionを返す
      //引数二つが同じ数値
      Assert.Throws<NoRecordFoundException>(() => table.Get(400, 400));
      
      //引数に0
      Assert.Throws<NoRecordFoundException>(() => table.Get(0, 0));
      Assert.Throws<NoRecordFoundException>(() => table.Get(0, 400));
      Assert.Throws<NoRecordFoundException>(() => table.Get(400, 0));
      
      //引数にマイナス値
      Assert.Throws<NoRecordFoundException>(() => table.Get(-400, -400));
      Assert.Throws<NoRecordFoundException>(() => table.Get(400, -400));
      Assert.Throws<NoRecordFoundException>(() => table.Get(-400, 400));
      
      //引数にマイナス値と0
      Assert.Throws<NoRecordFoundException>(() => table.Get(-400, 0));
      Assert.Throws<NoRecordFoundException>(() => table.Get(0, -400));
    }
  }
}