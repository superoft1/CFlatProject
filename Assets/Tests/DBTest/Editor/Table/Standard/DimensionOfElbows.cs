using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfElbows
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      //正常系 最小径(ElbowType = Long 90°)
      {
        var records = table.GetOne(15);
        Assert.AreEqual(38, records.CenterToEnd, delta);
      }

      //正常系 最大径(ElbowType = Long 90°)
      {
        var records = table.GetOne(1200);
        Assert.AreEqual(1829, records.CenterToEnd, delta);
      }

      //正常系 最小径(ElbowType = Long 45°)
      {
        var records = table.GetOne(15, elbowType:"45deg");
        Assert.AreEqual(16, records.CenterToEnd, delta);
      }

      //正常系 最大径(ElbowType = Long 45°)
      {
        var records = table.GetOne(1200, elbowType: "45deg");
        Assert.AreEqual(759, records.CenterToEnd, delta);
      }

      //正常系 最小径(ElbowType = Short 90°)
      {
        var records = table.GetOne(25, elbowType: "S90deg");
        Assert.AreEqual(25, records.CenterToEnd, delta);
      }

      //正常系 最大径(ElbowType = Short 90°)
      {
        var records = table.GetOne(600, elbowType: "S90deg");
        Assert.AreEqual(610, records.CenterToEnd, delta);
      }

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す(ElbowType = Long 90°)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0));

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す(ElbowType = Long 45°)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, elbowType: "45deg"));

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す(ElbowType = Short 90°)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, elbowType: "S90deg"));

    }

    /// <summary>
    /// NPSとCenterToEndを入力して、入力したCenterToEndに近いエルボータイプ(90度)のレコードを返す
    /// </summary>
    [Test]
    public void GetOne90ElbowByNPSAndCenterToEnd()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      //正常系 Longを返すケース
      {
        var record = table.GetOne90(25, 38.0f);
        Assert.AreEqual("90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //正常系 Longを返すケース
      {
        var record = table.GetOne90(25, 32.0f);
        Assert.AreEqual("90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //正常系 Shortを返すケース
      {
        var record = table.GetOne90(25, 25.0f);
        Assert.AreEqual("S90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //正常系 Shortを返すケース
      {
        var record = table.GetOne90(25, 31.0f);
        Assert.AreEqual("S90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //正常系 45度は返さない
      {
        var record = table.GetOne90(25, 10.0f);
        Assert.AreEqual("S90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //正常系 近さが同一なら、Longを返す
      {
        var record = table.GetOne90(25, 31.5f);
        Assert.AreEqual("90deg", record.ElbowType);
        Assert.AreEqual(25, record.NPSmm);
      }

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetOne90(3, 10.0f));
    }
  }
}
