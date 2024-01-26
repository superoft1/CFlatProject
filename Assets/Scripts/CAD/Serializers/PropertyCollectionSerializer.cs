using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.IO ;

namespace Chiyoda.CAD.Serializers
{
  internal class PropertyCollectionSerializer : AbstractReadIntoSerializer<PropertyCollection>
  {
    protected override bool Write( SerializationContext con, PropertyCollection obj )
    {
      foreach ( var prop in obj.UserDefinedProperties ) {
        if ( false == con.WriteValue( "property", prop ) ) return false ;
      }

      return true ;
    }

    protected override bool ReadInto( DeserializationContext con, PropertyCollection obj )
    {
      while ( con.GetNextElementName( out var name ) && name == "property" ) {
        if ( ! con.ReadValue<IUserDefinedNamedProperty>( "property", typeof( IUserDefinedNamedProperty ), prop => obj.Add( prop ) ) ) return false ;
      }

      return true ;
    }
  }
}