using System.Collections.Generic ;
using System.Data;
using System.IO;
using System.Data.Linq;
using Mono.Data.Sqlite ;

namespace Chiyoda.DB
{
  public class DbSqLite : DB
  {
    internal DbSqLite(DBConnection dbConn):base(dbConn) { }

    private string GetDBFilePath( string dbName ){
      return Path.Combine(_dbConn.SQLiteDirectory, dbName + ".db");
    }

    internal override IDbConnection CreateDBConnection( string dbName )
    {
      var DBFilePath = GetDBFilePath( dbName );
      if ( ! System.IO.File.Exists( DBFilePath ) )
        throw new FileNotFoundException( $"Cannot find db file : {DBFilePath}" ) ;
      return new SqliteConnection( "URI=file:" + DBFilePath ) ;
    }

    internal override IEnumerable<TEntity> GetTable<TEntity>( DataContext dataContext )
    {
      return dataContext.GetTable<TEntity>() ;
    }

    public const string ConectDB_Default = "VTP_Default";
    public const string ConectDB_Standard = "VTP_Standard";
    public const string ConectDB_Plot = "VTP_Plot";
    public const string ConectDB_PDRef = "PDRef";

  }
}