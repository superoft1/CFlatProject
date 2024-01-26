using Chiyoda;
using Chiyoda.DB ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Core;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.CAD.Model
{
  class DiameteLevelTest
  {
    [Test]
    public void Test()
    {
      // setupDB
      var sqliteFilePath = System.IO.Path.Combine( System.Environment.CurrentDirectory, @"Assets\StreamingAssets\DB" );
      var dbConn = new DBConnection()
      {
        DBType = DBType.SQLite,
        SQLiteDirectory = sqliteFilePath
      } ;
      DB.SetDBConnection( dbConn );
      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      var delta = 1e-4 ;

      //レベルの概念を廃止するのでコメントアウト
      //Assert.AreEqual(1, doc.DiameterLevel.GetMinDiameterLevel());
      //Assert.AreEqual(29, doc.DiameterLevel.GetMaxDiameterLevel());

      //Assert.AreEqual("1/2", doc.DiameterLevel.GetInchiDisp(1));
      //Assert.AreEqual("40", doc.DiameterLevel.GetInchiDisp(29));

      //// TODO UnitExtensionを使いたいなー
      //Assert.AreEqual(21.3, doc.DiameterLevel.FromLevel(1).OutsideMeter / 0.001, delta);
      //Assert.AreEqual(26.7, doc.DiameterLevel.FromLevel(2).OutsideMeter / 0.001, delta);
      //Assert.AreEqual(965.0, doc.DiameterLevel.FromLevel(28).OutsideMeter / 0.001, delta);
      //Assert.AreEqual(1016.0, doc.DiameterLevel.FromLevel(29).OutsideMeter / 0.001, delta);

      //Assert.AreEqual(1, doc.DiameterLevel.FromOutsideMeter(21.3 * 0.001 ).Level);
      //Assert.AreEqual(2, doc.DiameterLevel.FromOutsideMeter(26.7 * 0.001 ).Level);
      //Assert.AreEqual(28, doc.DiameterLevel.FromOutsideMeter(965.0 * 0.001 ).Level);
      //Assert.AreEqual(29, doc.DiameterLevel.FromOutsideMeter(1016.0 * 0.001 ).Level);
    }
  }
}