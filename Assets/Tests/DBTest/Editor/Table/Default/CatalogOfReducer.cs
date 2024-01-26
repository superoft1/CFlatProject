using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class CatalogOfReducer
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<CatalogOfReducerTable>();
      Assert.IsNotNull(table);

      // Specを指定して取得

      // 正常系 最小 PipeThickness(NPS_H) 20mm
      {
        var rec = table.Get(NPS_L_mm: 20, endPrep: "BW", identificationNote: "STD");
        //CRE
        var rec_c = rec.Where(r => r.ShortCode == "CRE").ToList();
        Assert.AreEqual(2, rec_c.Count);
        Assert.AreEqual("CRE_ASME_PT20/NPSS10_STD_BW", rec_c[0].IdentCode);
        Assert.AreEqual("CRE_ASME_PT20/NPSS15_STD_BW", rec_c[1].IdentCode);

        //ERE
        var rec_e = rec.Where(r => r.ShortCode == "ERE").ToList();
        Assert.AreEqual(2, rec_e.Count);
        Assert.AreEqual("ERE_ASME_PT20/NPSS10_STD_BW", rec_e[0].IdentCode);
        Assert.AreEqual("ERE_ASME_PT20/NPSS15_STD_BW", rec_e[1].IdentCode);
      }

      // 正常系 最大 PipeThickness(NPS_H) 600mm
      {
        var rec = table.Get(NPS_L_mm: 600, endPrep: "BW", identificationNote: "STD");
        //CRE
        var rec_c = rec.Where(r => r.ShortCode == "CRE").ToList();
        Assert.AreEqual(4, rec_c.Count);
        Assert.AreEqual("CRE_ASME_PT600/NPSS400_STD_BW", rec_c[0].IdentCode);
        Assert.AreEqual("CRE_ASME_PT600/NPSS450_STD_BW", rec_c[1].IdentCode);
        Assert.AreEqual("CRE_ASME_PT600/NPSS500_STD_BW", rec_c[2].IdentCode);
        Assert.AreEqual("CRE_ASME_PT600/NPSS550_STD_BW", rec_c[3].IdentCode);

        //ERE
        var rec_e = rec.Where(r => r.ShortCode == "ERE").ToList();
        Assert.AreEqual(4, rec_e.Count);
        Assert.AreEqual("ERE_ASME_PT600/NPSS400_STD_BW", rec_e[0].IdentCode);
        Assert.AreEqual("ERE_ASME_PT600/NPSS450_STD_BW", rec_e[1].IdentCode);
        Assert.AreEqual("ERE_ASME_PT600/NPSS500_STD_BW", rec_e[2].IdentCode);
        Assert.AreEqual("ERE_ASME_PT600/NPSS550_STD_BW", rec_e[3].IdentCode);
      }

      // 異常系 データが存在しない場合はNoRecordFoundExceptionを返す
      {
        //PipeThickness(NPS_H) 125mm
        Assert.Throws<NoRecordFoundException>(() => table.Get(NPS_L_mm: 125, endPrep: "BW", identificationNote: "STD"));

      }
    }
  }
}