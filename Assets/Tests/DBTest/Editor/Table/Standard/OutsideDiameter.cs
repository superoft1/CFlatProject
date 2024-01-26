using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class OutsideDiameter
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    /// <summary>
    /// NPS(mm)を入力して外径(OutsideDiameter)を取得する
    /// </summary>
    [Test]
    public void GetOne()
    {
      var table = Chiyoda.DB.DB.Get<OutsideDiameterTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      // ASME規格
      {
        // 正常系 最小のNPS
        {
          var min = table.GetOne(6);
          Assert.AreEqual(10.3, min.OutsideDiameter, delta);
        }
        // 正常系 最大のNPS
        {
          var max = table.GetOne(2000);
          Assert.AreEqual(2032, max.OutsideDiameter, delta);
        }
        // 異常系 存在しないNPS
        {
          Assert.Throws<NoRecordFoundException>(() => table.GetOne(0));
        }
      }

      // JIS規格
      {
        // 正常系 最小のNPS
        {
          var min = table.GetOne(6, "JIS");
          Assert.AreEqual(10.5, min.OutsideDiameter, delta);
        }
        // 正常系 最大のNPS
        {
          var max = table.GetOne(650, "JIS");
          Assert.AreEqual(660.4, max.OutsideDiameter, delta);
        }
        // 異常系 存在しないNPS
        {
          Assert.Throws<NoRecordFoundException>(() => table.GetOne(0,"JIS"));
        }
      }

    }

    /// <summary>
    /// インプットされた外径から最も近しいNPS規格のレコードを返すクラスのテスト
    /// </summary>
    [Test]
    public void GetClosestToNPS()
    {
      var table = Chiyoda.DB.DB.Get<OutsideDiameterTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      //インプットされる外径(mm)にもっとも近しいNPS規格のレコードを返す

      // ASME 規格
      {
        // 正常系 最小のNPSを取得するパターン (0.0mm ～ 12.0mm)
        // NPS [6(mm), 0.125(Inchi) , 1/8(InchiDisp) , 10.3(OutsideDiameter)]
        {
          var records = table.GetClosestTo(0.0);
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(0.125, records.Inchi, delta);
          Assert.AreEqual("1/8", records.InchiStr);
          Assert.AreEqual(10.3, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(12.0);
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(0.125, records.Inchi, delta);
          Assert.AreEqual("1/8", records.InchiStr);
          Assert.AreEqual(10.3, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に小さいNPSを取得する (12.1mm ～ 15.4mm)
        // NPS [8(mm), 0.25(Inchi) , 1/4(InchiDisp) , 13.7(OutsideDiameter)]
        {
          var records = table.GetClosestTo(12.1);
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(0.25, records.Inchi, delta);
          Assert.AreEqual("1/4", records.InchiStr);
          Assert.AreEqual(13.7, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(15.4);
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(0.25, records.Inchi, delta);
          Assert.AreEqual("1/4", records.InchiStr);
          Assert.AreEqual(13.7, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に大きいNPSを取得する (1879.6mm ～ 1981.0mm)
        // NPS [1900(mm), 76(Inchi) , 76(InchiDisp) , 1930(OutsideDiameter)]
        {
          var records = table.GetClosestTo(1879.6);
          Assert.AreEqual(1900, records.NPSmm, delta);
          Assert.AreEqual(76, records.Inchi, delta);
          Assert.AreEqual("76", records.InchiStr);
          Assert.AreEqual(1930, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(1981.0);
          Assert.AreEqual(1900, records.NPSmm, delta);
          Assert.AreEqual(76, records.Inchi, delta);
          Assert.AreEqual("76", records.InchiStr);
          Assert.AreEqual(1930, records.OutsideDiameter, delta);
        }

        // 正常系 最大のNPSを取得するパターン (1981.1mm ～ ∞)
        // NPS [2000(mm), 80(Inchi) , 80(InchiDisp) , 2032(OutsideDiameter)]
        {
          var records = table.GetClosestTo(1981.1);
          Assert.AreEqual(2000, records.NPSmm, delta);
          Assert.AreEqual(80, records.Inchi, delta);
          Assert.AreEqual("80", records.InchiStr);
          Assert.AreEqual(2032, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(100000000);
          Assert.AreEqual(2000, records.NPSmm, delta);
          Assert.AreEqual(80, records.Inchi, delta);
          Assert.AreEqual("80", records.InchiStr);
          Assert.AreEqual(2032, records.OutsideDiameter, delta);
        }

        // 異常系 入力された外径がマイナスの値である場合 NoRecordFoundExceptionを返す
        Assert.Throws<NoRecordFoundException>(() => table.GetClosestTo(-1));
    }

      // JIS 規格
      {
        // 正常系 最小のNPSを取得するパターン (0.0mm ～ 12.1mm)
        // NPS [6(mm), 0.125(Inchi) , 1/8(InchiDisp) , 10.5(OutsideDiameter)]
        {
          var records = table.GetClosestTo(0.0, "JIS");
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(0.125, records.Inchi, delta);
          Assert.AreEqual("1/8", records.InchiStr);
          Assert.AreEqual(10.5, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(12.1, "JIS");
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(0.125, records.Inchi, delta);
          Assert.AreEqual("1/8", records.InchiStr);
          Assert.AreEqual(10.5, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に小さいNPSを取得する (12.2mm ～ 15.5mm)
        // NPS [8(mm), 0.25(Inchi) , 1/4(InchiDisp) , 13.8(OutsideDiameter)]
        {
          var records = table.GetClosestTo(12.2, "JIS");
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(0.25, records.Inchi, delta);
          Assert.AreEqual("1/4", records.InchiStr);
          Assert.AreEqual(13.8, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(15.5, "JIS");
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(0.25, records.Inchi, delta);
          Assert.AreEqual("1/4", records.InchiStr);
          Assert.AreEqual(13.8, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に大きいNPSを取得する (584.2mm ～ 635.0mm)
        // NPS [600(mm), 24(Inchi) , 24(InchiDisp) , 609.6(OutsideDiameter)]
        {
          var records = table.GetClosestTo(584.2, "JIS");
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(24, records.Inchi, delta);
          Assert.AreEqual("24", records.InchiStr);
          Assert.AreEqual(609.6, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(635.0, "JIS");
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(24, records.Inchi, delta);
          Assert.AreEqual("24", records.InchiStr);
          Assert.AreEqual(609.6, records.OutsideDiameter, delta);
        }

        // 正常系 最大のNPSを取得するパターン (635.1mm ～ ∞)
        // NPS [650(mm), 26(Inchi) , 26(InchiDisp) , 660.4(OutsideDiameter)]
        {
          var records = table.GetClosestTo(635.1, "JIS");
          Assert.AreEqual(650, records.NPSmm, delta);
          Assert.AreEqual(26, records.Inchi, delta);
          Assert.AreEqual("26", records.InchiStr);
          Assert.AreEqual(660.4, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(100000000, "JIS");
          Assert.AreEqual(650, records.NPSmm, delta);
          Assert.AreEqual(26, records.Inchi, delta);
          Assert.AreEqual("26", records.InchiStr);
          Assert.AreEqual(660.4, records.OutsideDiameter, delta);
        }

        // 異常系 入力された外径がマイナスの値である場合 NoRecordFoundExceptionを返す
        Assert.Throws<NoRecordFoundException>(() => table.GetClosestTo(-1, "JIS"));
      }
    }
  }
}