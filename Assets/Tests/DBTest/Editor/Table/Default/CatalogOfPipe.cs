using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class CatalogOfPipe
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<CatalogOfPipeTable>();
      Assert.IsNotNull(table);

      // Specを指定して取得
      {
        var selected = table.Get(500, "BE", "STD");
        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("PIP_ASME_500_STD_BE", selected.First().IdentCode);
      }

      // TODO 異常系のテスト追加
    }
  }
}