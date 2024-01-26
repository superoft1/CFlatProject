using System ;
using System.Data ;
using System.IO ;
using System.Linq ;
using System.Text ;

namespace Importer.Equipment
{
  static class TableReader
  {
    public static DataTable Load( string path, string tableName, bool needPrimeKey = true )
    {
      Encoding encoding = Encoding.UTF8 ;
      var table = new DataTable( tableName );
      using ( var sr = new StreamReader( path, encoding ) ) {
        while ( !sr.EndOfStream ) {
          var line = sr.ReadLine();
          if ( string.IsNullOrEmpty( line ) || line.StartsWith( "#" ) ) {
            continue;
          }
          var cells = line.Split( ',' ).Select(p => p.Trim()).ToArray();

          if ( table.Columns.Count < cells.Length ) {
            for ( int i = table.Columns.Count ; i < cells.Length ; ++i ) {
              DataColumn workCol = table.Columns.Add( "col" + i, typeof( string ) );
              workCol.DefaultValue = "EOL";
            }
          }
          var newRow = table.NewRow();
          foreach ( var cell in cells.Select( ( value, index ) => new { value, index } ) ) {
            newRow[cell.index] = cell.value;
          }
          table.Rows.Add( newRow );
        }
      }

      if ( needPrimeKey ) {
        table.PrimaryKey = new[] {table.Columns[0]};
      }
      return table;
    }
  
    public static string[] Row2Array(DataRow row)
    {
      return row.ItemArray
        .Where( c => ! DBNull.Value.Equals( c ) )
        .Select( c => (string) c )
        .TakeWhile( s => s != "EOL" )
        .ToArray() ;
    }
  }
}