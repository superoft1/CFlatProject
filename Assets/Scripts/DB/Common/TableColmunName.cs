using System ;
using System.Collections.Generic;
using System.Data.Linq.Mapping ;

namespace Chiyoda.DB
{
  public class TableColmunName
  {
    public string Table { get ; }
    public List<string> Columns { get ; } = new List<string>();

    public TableColmunName( Type type )
    {
      if( Attribute.GetCustomAttribute( type, typeof(TableAttribute) ) is TableAttribute tableAttribute) {
        Table = tableAttribute.Name ;
      }
      else {
        throw new Exception($"Cannot get table attribute for {type.Name}" );
      }

      foreach ( var prop in type.GetProperties() ) {
        if ( Attribute.GetCustomAttribute( prop, typeof( ColumnAttribute ) ) is ColumnAttribute columnAttribute ) {
          Columns.Add( columnAttribute.Name );
        }
      }
    }
  }
}