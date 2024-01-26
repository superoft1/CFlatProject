using System.Linq;
using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class DimensionOfPipeShoe
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void Get()
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfPipeShoeTable>() ;
      Assert.IsNotNull( table );

      var delta = Tolerance.AssertEqualDelta;
      //����n �ŏ��a(ShoeType = Normal)
      {
        var records = table.GetOne(20, shoeType: "Normal");
        Assert.AreEqual(113, records.CenterToEnd, delta);
        Assert.AreEqual(100, records.Width, delta);
      }

      //����n �ő�a(ShoeType = Normal)
      {
        var records = table.GetOne(1200, shoeType: "Normal");
        Assert.AreEqual(760, records.CenterToEnd, delta);
        Assert.AreEqual(780, records.Width, delta);
      }

      //�ُ�n �f�[�^�����݂��Ȃ��ꍇ�ANoRecordFoundException��Ԃ�(shoeType = Normal)
      Assert.Throws<NoRecordFoundException>(() => table.GetOne(0, "Normal"));

    }
  }
}