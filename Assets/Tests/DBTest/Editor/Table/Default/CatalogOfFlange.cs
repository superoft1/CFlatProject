using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class CatalogOfFlange
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<CatalogOfFlangeTable>();
      Assert.IsNotNull(table);

      // Specを指定して取得
      {
        var selected = table.Get(500, 300, "BW", "RF");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("FLG_ASME_500_300_BW_RF", selected.First().IdentCode);
      }

      // TODO 異常系のテスト追加
    }
  }
}