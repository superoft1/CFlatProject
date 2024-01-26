using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class CatalogOfTee
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<CatalogOfTeeTable>();
      Assert.IsNotNull(table);

      // Specを指定して取得

      // 正常系 最小 PipeThickness(NPS_H) 15mm
      {
        var rec = table.Get(NPS_H_mm:15, endPrep:"BW", identificationNote:"STD");
        Assert.AreEqual(3, rec.Count);
        Assert.AreEqual("TEE_ASME_PT15/NPSB8_STD_BW", rec[0].IdentCode);
        Assert.AreEqual("TEE_ASME_PT15/NPSB10_STD_BW", rec[1].IdentCode);
        Assert.AreEqual("TEE_ASME_PT15/NPSB15_STD_BW", rec[2].IdentCode);
      }

      // 正常系 最大 PipeThickness(NPS_H) 600mm
      {
        var rec = table.Get(NPS_H_mm: 600, endPrep: "BW", identificationNote: "STD");
        Assert.AreEqual(8, rec.Count);
        Assert.AreEqual("TEE_ASME_PT600/NPSB250_STD_BW", rec[0].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB300_STD_BW", rec[1].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB350_STD_BW", rec[2].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB400_STD_BW", rec[3].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB450_STD_BW", rec[4].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB500_STD_BW", rec[5].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB550_STD_BW", rec[6].IdentCode);
        Assert.AreEqual("TEE_ASME_PT600/NPSB600_STD_BW", rec[7].IdentCode);
      }

      // 異常系 データが存在しない場合はNoRecordFoundExceptionを返す
      {
        //PipeThickness(NPS_H) 125
        Assert.Throws<NoRecordFoundException>(() => table.Get(NPS_H_mm: 125, endPrep: "BW", identificationNote: "STD"));

      }
    }
  }
}