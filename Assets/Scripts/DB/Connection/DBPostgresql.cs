using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Npgsql;

namespace Chiyoda.DB
{
  public class DBPostgresql : DB
  {
    internal DBPostgresql(DBConnection dbConn):base(dbConn) { }

    internal override IDbConnection CreateDBConnection(string dbName)
    {
      _dbConn.Database = dbName ;
      return new NpgsqlConnection( _dbConn.ConnStr ) ;
    }
    
    internal override IEnumerable<TEntity> GetTable<TEntity>(DataContext dataContext) 
    {
      // DataContext.GetTableで生成されるSQL文はPostgreSQLでは対応していないので、自分でSQL文を生成する
      var tableColmunNames = new TableColmunName(typeof( TEntity )) ;
      var selectSql = CreateSelectSql(tableColmunNames);
      return dataContext.ExecuteQuery<TEntity>(selectSql);
    }

    private static string CreateSelectSql( TableColmunName tableColmunNames )
    {
      var sql = "SELECT " ;
      // PostgreSQLは列とテーブル名をダブルコーテーションで囲む
      sql += string.Join( ", ", tableColmunNames.Columns.Select( item =>  "\"" + item + "\"" ) ) ;
      sql += $" FROM \"{tableColmunNames.Table}\"" ;
      return sql ;
    }
  }
}