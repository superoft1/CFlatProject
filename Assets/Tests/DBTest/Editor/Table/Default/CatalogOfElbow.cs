using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class CatalogOfElbow
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<CatalogOfElbowTable>();
      Assert.IsNotNull(table);

      // Specを指定して取得

      // 正常系 90E 最小
      {
        var selected = table.Get(NPSmm:6, endPrep:"BW", identificationNote:"STD");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("90E_ASME_6_STD_BW", selected.First().IdentCode);
      }
      // 正常系 90E 最大
      {
        var selected = table.Get(NPSmm: 600, endPrep: "BW", identificationNote: "STD");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("90E_ASME_600_STD_BW", selected.First().IdentCode);
      }
      // 正常系 45E 最小
      {
        var selected = table.Get(NPSmm: 6, endPrep: "BW", identificationNote: "STD", elbow:"45E");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("45E_ASME_6_STD_BW", selected.First().IdentCode);
      }
      // 正常系 45E 最大
      {
        var selected = table.Get(NPSmm: 600, endPrep: "BW", identificationNote: "STD", elbow: "45E");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("45E_ASME_600_STD_BW", selected.First().IdentCode);
      }

      // 異常系 存在しないはNoRecordFoundExceptionを返す
      {
        //NPSmm 125
        Assert.Throws<NoRecordFoundException>(() => table.Get(NPSmm: 125, endPrep: "BW", identificationNote: "STD"));
        Assert.Throws<NoRecordFoundException>(() => table.Get(NPSmm: 125, endPrep: "BW", identificationNote: "STD", elbow: "45E"));
      }
    }
  }
}