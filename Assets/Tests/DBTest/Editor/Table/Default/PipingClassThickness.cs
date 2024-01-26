using Chiyoda.DB;
using System.Linq;
using Chiyoda.DB.Entity;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class PipingClassThickness
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassThicknessTable>();
      Assert.NotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      // 正常系 CL150
      {
        var thicknessList = table.Get("CL150");
        Assert.NotNull(thicknessList);
        Assert.NotZero(thicknessList.Count);

        {
          var thickness = thicknessList.First(rec => rec.NPSmm == 6);
          Assert.AreEqual("STD", thickness.IdentificationNote);
          Assert.AreEqual(40, thickness.ScheduleNo);
          Assert.AreEqual(1.73, thickness.Thickness, delta);
//          Assert.AreEqual(0.37, thickness.PlainEndWeightMass_kg, delta);
        }
        {
          var thickness = thicknessList.First(rec => rec.NPSmm == 600);
          Assert.AreEqual("STD", thickness.IdentificationNote);
          Assert.AreEqual(20, thickness.ScheduleNo);
          Assert.AreEqual(9.53, thickness.Thickness, delta);
//          Assert.AreEqual(141.12, thickness.PlainEndWeightMass_kg, delta);
        }
      }

      // TODO 正常系 CL2500
      {

      }

      // 異常系 -> 要素数ゼロのリストを返す
      // TODO RecordNotFoundExceptionにする
      {
        var thicknessList = table.Get("XXX");
        Assert.NotNull(thicknessList);
        Assert.Zero(thicknessList.Count);
      }

    }

    [Test]
    public void GetOne()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassThicknessTable>();
      Assert.NotNull(table);

      var delta = Tolerance.AssertEqualDelta;

      Assert.AreEqual("STD", table.GetOne("CL150", 6).IdentificationNote);
      Assert.AreEqual(1.73, table.GetOne("CL150", 6).Thickness, delta);
      Assert.AreEqual("STD", table.GetOne("CL150", 600).IdentificationNote);
      Assert.AreEqual(9.53, table.GetOne("CL150", 600).Thickness, delta);
    }
  }
}
