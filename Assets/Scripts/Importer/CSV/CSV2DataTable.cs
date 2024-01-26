using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer.CSV
{
  internal static class CSV2DataTable
  {
    public static DataTable Load( string path, Encoding encoding, string tableName )
    {
      var bHeader = true ;
      var table = new DataTable( tableName ) ;
      using ( var sr = new System.IO.StreamReader( path, encoding ) ) {
        while ( ! sr.EndOfStream ) {
          var line = sr.ReadLine() ;
          var cells = line.Split( ',' ) ;

          if ( bHeader ) {
            foreach ( var cell in cells ) {
              table.Columns.Add( cell.Trim( '\"' ) ) ;
            }

            bHeader = false ;
          }
          else {
            var newRow = table.NewRow() ;
            foreach ( var cell in cells.Select( ( value, index ) => new { value, index } ) ) {
              newRow[ cell.index ] = cell.value ;
            }

            table.Rows.Add( newRow ) ;
          }
        }
      }

      return table ;
    }
  }
}

