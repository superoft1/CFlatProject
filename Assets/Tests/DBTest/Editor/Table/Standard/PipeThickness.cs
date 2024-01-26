using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class PipeThickness
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<PipeThicknessTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;

      //正常系 最小径 6mm ASME
      {
        var records = table.Get(6);
        Assert.AreEqual(6, records.Count());

        Assert.AreEqual(null, records[0].IdentificationNote);
        Assert.AreEqual(10, records[0].ScheduleNo);
        Assert.AreEqual(1.24, records[0].WallThickness_mm, delta);

        Assert.AreEqual("XXS", records[5].IdentificationNote);
        Assert.AreEqual(null, records[5].ScheduleNo);
        Assert.AreEqual(4.83, records[5].WallThickness_mm, delta);
      }

      //正常系 最大径 2000mm ASME
      {
        var records = table.Get(2000);
        Assert.AreEqual(12, records.Count());

        Assert.AreEqual(null, records[0].IdentificationNote);
        Assert.AreEqual(null, records[0].ScheduleNo);
        Assert.AreEqual(14.27, records[0].WallThickness_mm, delta);

        Assert.AreEqual(null, records[11].IdentificationNote);
        Assert.AreEqual(null, records[11].ScheduleNo);
        Assert.AreEqual(31.75, records[11].WallThickness_mm, delta);
      }

      //異常系 データが存在しない場合、NoRecordFoundExceptionを返す
      Assert.Throws<NoRecordFoundException>(() => table.Get(0));
    }
  }
}