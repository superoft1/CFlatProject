using Chiyoda.DB ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.DB
{
  public class PTRating
  {
    [SetUp]
    public void Init()
    {
      Setup.DB() ;
    }

    [Test]
    public void GetClass()
    {
      var table = Chiyoda.DB.DB.Get<PTRatingTable>() ;
      Assert.IsNotNull( table );

      {
        Assert.AreEqual(300, table.GetNP("1.1", 20.0, 40.0));
        Assert.AreEqual(600, table.GetNP("1.1", 20.0, 100.0));
        Assert.AreEqual(1500, table.GetNP("1.1", 20.0, 250.0));

        Assert.AreEqual(400, table.GetNP("1.1", 400.0, 40.0));
        Assert.AreEqual(900, table.GetNP("1.1", 400.0, 100.0));
        Assert.AreEqual(2500, table.GetNP("1.1", 400.0, 250.0));

        Assert.AreEqual(600, table.GetNP("1.1", 401.0, 40.0));
        Assert.AreEqual(1500, table.GetNP("1.1", 401.0, 100.0));
      }      
      //TODO IFの整合性をとったら、異常系の試験を追加
      
    }

    [Test]
    public void GetRating()
    {
      var table = Chiyoda.DB.DB.Get<STD_RatingClassTable>();
      Assert.IsNotNull(table);

      //ASME
      {
        var asme = table.GetRatingList();
        Assert.AreEqual(7, asme.Count);
        Assert.AreEqual(150, asme[0].Rating);
        Assert.AreEqual(300, asme[1].Rating);
        Assert.AreEqual(400, asme[2].Rating);
        Assert.AreEqual(600, asme[3].Rating);
        Assert.AreEqual(900, asme[4].Rating);
        Assert.AreEqual(1500, asme[5].Rating);
        Assert.AreEqual(2500, asme[6].Rating);
      }

      //存在しない規格
      {
        Assert.Throws<NoRecordFoundException>(() => table.GetRatingList("XXX"));
      }

    }

  }
}