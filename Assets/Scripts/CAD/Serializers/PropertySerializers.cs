using System ;
using System.Collections.Generic ;
using System.Globalization ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.IO ;

namespace Chiyoda.CAD.Serializers
{
  internal class UserDefinedNamedPropertySerializer : AbstractSerializer<UserDefinedNamedProperty>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in UserDefinedNamedProperty obj )
    {
      con.WriteValue( "name", obj.PropertyName ) ;
      con.WriteValue( "type", obj.Type ) ;
      con.WriteValue( "value", obj.Value ) ;
      if ( -double.MaxValue < obj.MinValue ) {
        con.WriteValue( "min", obj.MinValue ) ;
      }
      if ( obj.MaxValue < double.MaxValue ) {
        con.WriteValue( "max", obj.MaxValue ) ;
      }

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<UserDefinedNamedProperty> onRead )
    {
      string propName = null, type = null, value = null, min = null, max = null ;
      while ( con.GetNextElementName( out var name ) ) {
        switch ( name ) {
          case "name" :
            if ( null != propName ) return false ;
            if ( false == con.ReadString( "name", out propName ) ) return false ;
            break ;

          case "type" :
            if ( null != type ) return false ;
            if ( false == con.ReadString( "type", out type ) ) return false ;
            break ;

          case "value" :
            if ( null != value ) return false ;
            if ( false == con.ReadString( "value", out value ) ) return false ;
            break ;

          case "min" :
            if ( null != min ) return false ;
            if ( false == con.ReadString( "min", out min ) ) return false ;
            break ;

          case "max" :
            if ( null != max ) return false ;
            if ( false == con.ReadString( "max", out max ) ) return false ;
            break ;

          default : return false ;
        }
      }

      if ( null == propName || null == type || null == value ) return false ;
      if ( ! Enum.TryParse( type, out PropertyType propertyType ) ) return false ;
      if ( ! double.TryParse( value, out var defaultValue ) ) return false ;

      var minValue = -double.MaxValue ;
      var maxValue = +double.MaxValue ;
      if ( null != min ) {
        if ( ! double.TryParse( min, out minValue ) ) return false ;
      }
      if ( null != max ) {
        if ( ! double.TryParse( max, out maxValue ) ) return false ;
      }

      onRead( new UserDefinedNamedProperty( (IPropertiedElement) con.Parent, propName, propertyType, defaultValue, minValue, maxValue ) ) ;
      return true ;
    }
  }

    internal class UserDefinedSteppedNamedPropertySerializer : AbstractSerializer<UserDefinedSteppedNamedProperty>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in UserDefinedSteppedNamedProperty obj )
    {
      con.WriteValue( "name", obj.PropertyName ) ;
      con.WriteValue( "type", obj.Type ) ;
      con.WriteValue( "value", obj.Value ) ;
      if ( -double.MaxValue < obj.MinValue ) {
        con.WriteValue( "min", obj.MinValue ) ;
      }
      if ( obj.MaxValue < double.MaxValue ) {
        con.WriteValue( "max", obj.MaxValue ) ;
      }
      if ( 0 < obj.Step ) {
        con.WriteValue( "step", obj.Step ) ;
      }

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<UserDefinedSteppedNamedProperty> onRead )
    {
      string propName = null, type = null, value = null, min = null, max = null, step = null ;
      while ( con.GetNextElementName( out var name ) ) {
        switch ( name ) {
          case "name" :
            if ( null != propName ) return false ;
            if ( false == con.ReadString( "name", out propName ) ) return false ;
            break ;

          case "type" :
            if ( null != type ) return false ;
            if ( false == con.ReadString( "type", out type ) ) return false ;
            break ;

          case "value" :
            if ( null != value ) return false ;
            if ( false == con.ReadString( "value", out value ) ) return false ;
            break ;

          case "min" :
            if ( null != min ) return false ;
            if ( false == con.ReadString( "min", out min ) ) return false ;
            break ;

          case "max" :
            if ( null != max ) return false ;
            if ( false == con.ReadString( "max", out max ) ) return false ;
            break ;

          case "step" :
            if ( null != step ) return false ;
            if ( false == con.ReadString( "step", out step ) ) return false ;
            break ;

          default : return false ;
        }
      }

      if ( null == propName || null == type || null == value ) return false ;
      if ( ! Enum.TryParse( type, out PropertyType propertyType ) ) return false ;
      if ( ! double.TryParse( value, out var defaultValue ) ) return false ;

      var minValue = -double.MaxValue ;
      var maxValue = +double.MaxValue ;
      var stepValue = 0d ;
      if ( null != min ) {
        if ( ! double.TryParse( min, out minValue ) ) return false ;
      }
      if ( null != max ) {
        if ( ! double.TryParse( max, out maxValue ) ) return false ;
      }
      if ( null != step ) {
        if ( ! double.TryParse( step, out stepValue ) ) return false ;
      }

      onRead( new UserDefinedSteppedNamedProperty( (IPropertiedElement) con.Parent, propName, propertyType, defaultValue, minValue, maxValue, stepValue ) ) ;
      return true ;
    }
  }

  internal class UserDefinedEnumNamedPropertySerializer : AbstractSerializer<UserDefinedEnumNamedProperty>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in UserDefinedEnumNamedProperty obj )
    {
      con.WriteValue( "name", obj.PropertyName ) ;
      con.WriteValue( "type", obj.Type ) ;
      con.WriteValue( "value", obj.Value ) ;
      if ( null != obj.EnumValues ) {
        con.WriteValue( "list", Stringify( obj.EnumValues ) ) ;
      }

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<UserDefinedEnumNamedProperty> onRead )
    {
      string propName = null, type = null, value = null, list = null ;
      while ( con.GetNextElementName( out var name ) ) {
        switch ( name ) {
          case "name" :
            if ( null != propName ) return false ;
            if ( false == con.ReadString( "name", out propName ) ) return false ;
            break ;

          case "type" :
            if ( null != type ) return false ;
            if ( false == con.ReadString( "type", out type ) ) return false ;
            break ;

          case "value" :
            if ( null != value ) return false ;
            if ( false == con.ReadString( "value", out value ) ) return false ;
            break ;

          case "list" :
            if ( null != list ) return false ;
            if ( false == con.ReadString( "list", out list ) ) return false ;
            break ;

          default : return false ;
        }
      }

      if ( null == propName || null == type || null == value ) return false ;
      if ( ! Enum.TryParse( type, out PropertyType propertyType ) ) return false ;
      if ( ! double.TryParse( value, out var defaultValue ) ) return false ;

      Dictionary<string, double> dic = null ;
      if ( null != list ) {
        if ( ! Parse( list, out dic ) ) return false ;
      }

      onRead( new UserDefinedEnumNamedProperty( (IPropertiedElement) con.Parent, propName, propertyType, defaultValue, dic ) ) ;
      return true ;
    }

    private static string Stringify( IEnumerable<KeyValuePair<string, double>> enums )
    {
      return string.Join( ",", enums.Select( pair => $"{pair.Key.QuoteCSV()},{pair.Value.ToString( CultureInfo.InvariantCulture )}" ) ) ;
    }

    private static bool Parse( string list, out Dictionary<string, double> result )
    {
      result = null ;
      var dic = new Dictionary<string, double>() ;

      int startIndex = 0 ;
      string key = null ;
      while ( startIndex < list.Length ) {
        var str = list.UnquoteCsv( startIndex, out var nextIndex, out var endOfLine ) ;
        if ( null == key ) {
          key = str ;
          if ( endOfLine ) return false ;
        }
        else {
          if ( false == double.TryParse( str, out var value ) ) {
            return false ;
          }

          if ( dic.ContainsKey( key ) ) return false ;
          dic.Add( key, value ) ;
          key = null ;
        }

        startIndex = nextIndex ;
      }

      if ( null != key ) return false ;  // キーで終了

      result = dic ;
      return true ;
    }
  }

}