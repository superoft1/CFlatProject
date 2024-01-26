using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class Diameter
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void GetList()
    {
      var table = Chiyoda.DB.DB.Get<DiameterTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      // ASME CL150 default
      {
        var diameterList = table.GetList("CL150");
        Assert.NotNull(diameterList);
        Assert.AreEqual(23, diameterList.Count); // 現状、23個のみ登録している

        {
          var diameter = diameterList.First(rec => rec.NPSmm == 6);
          Assert.AreEqual("CL150", diameter.PipingClass);
          Assert.AreEqual(6, diameter.NPSmm);
          Assert.AreEqual(0.125, diameter.NPSInchi, delta);
          Assert.AreEqual("1/8", diameter.NPSInchiStr);
          Assert.AreEqual(10.3, diameter.OutsideDiameter, delta);
          Assert.AreEqual(1.73, diameter.WallThickness, delta);
          // TODO 必要性確認次第実装
          //          Assert.AreEqual("STD", diameter.IdentificationNote); 
          //          Assert.AreEqual(40, diameter.ScheduleNo);
        }
        {
          var diameter = diameterList.First(rec => rec.NPSmm == 600);
          Assert.AreEqual("CL150", diameter.PipingClass);
          Assert.AreEqual(600, diameter.NPSmm);
          Assert.AreEqual(24, diameter.NPSInchi, delta);
          Assert.AreEqual("24", diameter.NPSInchiStr);
          Assert.AreEqual(610.0, diameter.OutsideDiameter, delta);
          Assert.AreEqual(9.53, diameter.WallThickness, delta);
          // TODO 必要性確認次第実装
          //          Assert.AreEqual("STD", diameter.IdentificationNote);
          //          Assert.AreEqual(20, diameter.ScheduleNo);
        }
      }

      // TODO ASME CL2500
      /*
      {
      }
      */

      // TODO JIS_default
      /*
      { 
        var diameterList = table.GetList("JIS_Default");
        Assert.NotNull(diameterList);
        Assert.NotZero(diameterList.Count);

        {
          var diameter = diameterList.First(rec => rec.NPSmm == 6);
          Assert.AreEqual("JIS_Default", diameter.PipingClass);
          Assert.AreEqual(6, diameter.NPSmm);
          Assert.AreEqual(0.125, diameter.NPSInchi);
          Assert.AreEqual("1/8", diameter.NPSInchiStr);
          Assert.AreEqual(10.4, diameter.OutsideDiameter);
          Assert.AreEqual(1.73, diameter.WallThickness);
          //          Assert.AreEqual("STD", diameter.IdentificationNote);
          //          Assert.AreEqual(40, diameter.ScheduleNo);
        }
      }
      */

      // 異常系(存在しないサービスクラス -> 例外を返す)
      Assert.Throws<NoRecordFoundException>(() => table.GetList("XXXXX"));
    }

    [Test]
    public void GetSizeUp()
    {
      var table = Chiyoda.DB.DB.Get<DiameterTable>();
      Assert.IsNotNull(table);

      // ASME CL150 default
      {
        var diameter = table.GetSizeUp("CL150", 6);
        Assert.AreEqual(8, diameter.NPSmm);
      }

      // 異常系(それ以上アップできない-> 例外を返す)
      Assert.Throws<NoRecordFoundException>(() => table.GetSizeUp("XXXXX", 600));

      // 異常系(存在しないサービスクラス/存在しないNPS -> 例外を返す)
      Assert.Throws<NoRecordFoundException>(() => table.GetSizeUp("XXXXX", 6));
      Assert.Throws<NoRecordFoundException>(() => table.GetSizeUp("CL150", 2));
    }

    [Test]
    public void GetSizeDown()
    {
      var table = Chiyoda.DB.DB.Get<DiameterTable>();
      Assert.IsNotNull(table);

      // ASME CL150 default
      {
        var diameter = table.GetSizDown("CL150", 600);
        Assert.AreEqual(550, diameter.NPSmm);
      }

      // 異常系(それ以上アップできない-> 例外を返す)
      Assert.Throws<NoRecordFoundException>(() => table.GetSizDown("CL150", 6));

      // 異常系(存在しないサービスクラス/存在しないNPS -> 例外を返す)
      Assert.Throws<NoRecordFoundException>(() => table.GetSizDown("XXXXX", 600));
      Assert.Throws<NoRecordFoundException>(() => table.GetSizDown("CL150", 605));
    }


    /// <summary>
    /// インプットされた外径から最も近しいNPS規格のレコードを返すクラスのテスト
    /// </summary>
    [Test]
    public void GetClosestTo()
    {
      var table = Chiyoda.DB.DB.Get<DiameterTable>();
      Assert.IsNotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      //インプットされる外径(mm)にもっとも近しいNPS規格のレコードを返す

      // ASME 規格
      {
        // 正常系 最小のNPSを取得するパターン (0.0mm ～ 12.0mm)
        // NPS [6(mm), 10.3(OutsideDiameter)]
        {
          var records = table.GetClosestTo("CL150", 0.0);
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(10.3, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo("CL150", 12.0);
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(10.3, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に小さいNPSを取得する (12.1mm ～ 15.4mm)
        // NPS [8(mm), 13.7(OutsideDiameter)]
        {
          var records = table.GetClosestTo("CL150", 12.1);
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(13.7, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo("CL150", 15.4);
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(13.7, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に大きいNPSを取得する (1879.6mm ～ 584.0mm)
        // NPS [550(mm), 559(OutsideDiameter)]
        {
          var records = table.GetClosestTo("CL150", 534.0);
          Assert.AreEqual(550, records.NPSmm, delta);
          Assert.AreEqual(559, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo("CL150", 584.0);
          Assert.AreEqual(550, records.NPSmm, delta);
          Assert.AreEqual(559, records.OutsideDiameter, delta);
        }

        // 正常系 最大のNPSを取得するパターン (585.0mm ～ ∞)
        // NPS [600(mm), 610(OutsideDiameter)]
        {
          var records = table.GetClosestTo("CL150", 585.0);
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(610, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo("CL150", 100000000);
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(610, records.OutsideDiameter, delta);
        }

        // 異常系 入力された外径がマイナスの値である場合 NoRecordFoundExceptionを返す
        Assert.Throws<NoRecordFoundException>(() => table.GetClosestTo("CL150", -1));
      }

      // TODO JIS 規格
      /*
      {
        // 正常系 最小のNPSを取得するパターン (0.0mm ～ 12.1mm)
        // NPS [6(mm), 10.5(OutsideDiameter)]
        {
          var records = table.GetClosestTo(0.0, "JIS");
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(10.5, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(12.1, "JIS");
          Assert.AreEqual(6, records.NPSmm, delta);
          Assert.AreEqual(10.5, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に小さいNPSを取得する (12.2mm ～ 15.5mm)
        // NPS [8(mm), 13.8(OutsideDiameter)]
        {
          var records = table.GetClosestTo(12.2, "JIS");
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(13.8, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(15.5, "JIS");
          Assert.AreEqual(8, records.NPSmm, delta);
          Assert.AreEqual(13.8, records.OutsideDiameter, delta);
        }

        // 正常系 二番目に大きいNPSを取得する (584.2mm ～ 635.0mm)
        // NPS [600(mm), 609.6(OutsideDiameter)]
        {
          var records = table.GetClosestTo(584.2, "JIS");
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(609.6, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(635.0, "JIS");
          Assert.AreEqual(600, records.NPSmm, delta);
          Assert.AreEqual(609.6, records.OutsideDiameter, delta);
        }

        // 正常系 最大のNPSを取得するパターン (635.1mm ～ ∞)
        // NPS [650(mm), 660.4(OutsideDiameter)]
        {
          var records = table.GetClosestTo(635.1, "JIS");
          Assert.AreEqual(650, records.NPSmm, delta);
          Assert.AreEqual(660.4, records.OutsideDiameter, delta);
        }
        {
          var records = table.GetClosestTo(100000000, "JIS");
          Assert.AreEqual(650, records.NPSmm, delta);
          Assert.AreEqual(660.4, records.OutsideDiameter, delta);
        }

        // 異常系 入力された外径がマイナスの値である場合 NoRecordFoundExceptionを返す
        Assert.Throws<NoRecordFoundException>(() => table.GetClosestTo(-1, "JIS"));
      }
      */
    }
  }
}
