using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfTees
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfTeesTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      //正常系 NPS_HとNPS_Bが同一径のケース（Table6.1-7） 最小パターン抽出
      {
        var records = table.GetOne(15, 15);
        Assert.AreEqual(25, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(25, records.CenterToEnd_Outlet_M, delta);
      }

      //正常系 NPS_HとNPS_Bが同一径のケース（Table6.1-7） 最大パターン抽出
      {
        var records = table.GetOne(1200, 1200);
        Assert.AreEqual(889, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(838, records.CenterToEnd_Outlet_M, delta);
      }

      //正常系 NPS_HとNPS_Bが異なる径のケース（Table6.1-8） 最小パターン抽出
      {
        var records = table.GetOne(15, 8);
        Assert.AreEqual(25, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(25, records.CenterToEnd_Outlet_M, delta);
      }

      //正常系 NPS_HとNPS_Bが異なるケース（Table6.1-8） 最大パターン抽出
      {
        var records = table.GetOne(1200, 1150);
        Assert.AreEqual(889, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(838, records.CenterToEnd_Outlet_M, delta);
      }

      /////////////////// 暫定処置 /////////////////////////
      // アプリ側で規格外のNPS組み合わせでも外挿してデータを返して欲しい要望があったので、対応
      {
        var records = table.GetOne(1200, 15);
        Assert.AreEqual(889, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(647, records.CenterToEnd_Outlet_M, delta);
      }
      {
        var records = table.GetOne(50,8);
        Assert.AreEqual(64, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(39, records.CenterToEnd_Outlet_M, delta);
      }
      {
        var records = table.GetOne(40, 6);
        Assert.AreEqual(57, records.CenterToEnd_Run_C, delta);
        Assert.AreEqual(57, records.CenterToEnd_Outlet_M, delta);
      }

      // HeaderよりBranchが大きい場合はException
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(15, 50));
      //////////////////////////////////////////////////////

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, 0));
    }
  }
}