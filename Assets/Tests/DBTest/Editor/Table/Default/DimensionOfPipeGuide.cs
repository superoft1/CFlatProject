using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfPipeGuide
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfPipeGuideTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;
      //����n �ŏ��a(GuideType = GP)
      {
        var records = table.GetOne(20, guideType: "GP");
        Assert.AreEqual(50, records.Height, delta);
        Assert.AreEqual(30, records.Width, delta);
      }

      //����n �ő�a(GuideType = GP)
      {
        var records = table.GetOne(950, guideType: "GP");
        Assert.AreEqual(580, records.Height, delta);
        Assert.AreEqual(100, records.Width, delta);
      }

      //����n �ŏ��a(GuideType = GPS)
      {
        var records = table.GetOne(20, guideType: "GPS");
        Assert.AreEqual(20, records.Height, delta);
        Assert.AreEqual(5, records.Width, delta);
      }

      //����n �ő�a(GuideType = GPS)
      {
        var records = table.GetOne(1200, guideType: "GPS");
        Assert.AreEqual(48, records.Height, delta);
        Assert.AreEqual(60, records.Width, delta);
      }

      //�ُ�n �f�[�^�����݂��Ȃ��ꍇ�ANoRecordFoundException��Ԃ�(guideType = GP)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, "GP"));

      //�ُ�n �f�[�^�����݂��Ȃ��ꍇ�ANoRecordFoundException��Ԃ�(guideType = GPS)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, "GPS"));
    }
  }
}