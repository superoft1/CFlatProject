using Chiyoda.DB ;

namespace Tests.EditorMode.Editor.DB
{
  public class Setup
  {
    public static void DB()
    {
      var sqliteFilePath = System.IO.Path.Combine( System.Environment.CurrentDirectory, @"Assets/StreamingAssets/DB" );
      var dbConn = new DBConnection()
      {
        DBType = DBType.SQLite,
        SQLiteDirectory = sqliteFilePath
      } ;
      Chiyoda.DB.DB.SetDBConnection( dbConn );
    }
  }
}