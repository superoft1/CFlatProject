using System;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;

namespace Chiyoda.DB
{
  public abstract class DB
  {
    protected readonly DBConnection _dbConn = null ;
    
    private static DB _instance = null ;

    internal DB( DBConnection dbConn ) { _dbConn = dbConn ; }

    public static void SetDBConnection(DBConnection dbConn)
    {
      switch ( dbConn.DBType ) {
        case DBType.SQLite:
          _instance = new DbSqLite(dbConn) ; 
          break;
        case DBType.PostgreSQL:
          _instance = new DBPostgresql(dbConn) ; 
          break;
        default:
          throw new Exception("Invalid DB Type") ;
      }
    }
    
    public static T Get<T>() where T : TableBase, new()
    {
      if (_instance == null) {
        throw new Exception("DB is not initialized!") ;
      }
      var tblType = typeof(T);
      if ( ! _tableCache.ContainsKey( tblType ) ) {
        _tableCache[ tblType ] = new T() ;
        _tableCache[ tblType ].Init( _instance ) ;
      }
      return _tableCache[ tblType ] as T;
    }
    
    internal abstract IDbConnection CreateDBConnection(string dbName) ;
    
    internal abstract IEnumerable<TEntity> GetTable<TEntity>( DataContext dataContext ) where TEntity : class, new() ;
    
    protected static readonly Dictionary<Type, TableBase> _tableCache = new Dictionary<Type, TableBase>();
  }
}