namespace Chiyoda.DB
{
  public enum DBType
  {
    SQLite,
    PostgreSQL
  } ;
  
  public class DBConnection
  {
    public DBType DBType { get ; set ; }
    
    public string SQLiteDirectory { get ; set ; }

    // ひとまず、ハードコーディング、VTPのコンフィグファイルにいずれ入れ込む
    public string Host => "127.0.0.1" ;
    public string Database { get; set; } = "" ;
    public int Port => 5432;
    public string Username => "VTPUser" ;
    public string Password => "VTPUser" ;

    public string ConnStr
    {
      get { return $"Host={Host};Database={Database};Port={Port};Username={Username};Password={Password}" ; }
    }
  }
}