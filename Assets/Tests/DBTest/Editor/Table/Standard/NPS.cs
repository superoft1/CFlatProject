using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class NPS
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void GetList()
    {
      var table = Chiyoda.DB.DB.Get<NPSTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      // Get NPS 6mm
      var NPS6mm = table.Records.Where(rec => rec.mm == 6);
      Assert.AreEqual(1, NPS6mm.Count());
      Assert.AreEqual(0.125, NPS6mm.First().Inchi, delta);
      Assert.AreEqual("1/8", NPS6mm.First().InchiStr);

      // 一覧を取得
      var recordList = table.Records.OrderBy(rec => rec.Inchi).ToList();
      Assert.AreEqual(44, recordList.Count());

      // 0.5 inchi = 15 mm
      Assert.AreEqual(0.5, recordList[3].Inchi, delta); // 呼び径Inchi
      Assert.AreEqual(15, recordList[3].mm); // 呼び径mm

      // 2.0 inchi = 50 mm
      Assert.AreEqual(2.0, recordList[8].Inchi, delta);
      Assert.AreEqual(50, recordList[8].mm);
    }

    /// <summary>
    /// インプットされた外径から最も近しいNPS規格のレコードを返すクラスのテスト
    /// </summary>
    [Test]
    public void GetDiffIndexBetween()
    {
      var table = Chiyoda.DB.DB.Get<NPSTable>();
      Assert.IsNotNull(table);

      // 正常系 2サイズアップ
      {
        var diffIndex = table.GetDiffIndexBetween(6, 10);
        Assert.AreEqual(2, diffIndex);
      }
      // 正常系 2サイズダウン
      {
        var diffIndex = table.GetDiffIndexBetween(2000, 1800);
        Assert.AreEqual(-2, diffIndex);
      }

      // 異常系 対応するNPSがない場合 NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetDiffIndexBetween(10, 5));
      Assert.Throws<NoRecordFoundException>(() => table.GetDiffIndexBetween(11, 6));
    }

  }
}