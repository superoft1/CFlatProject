using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Globalization ;
using System.Linq.Expressions ;
using System.Xml ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using JetBrains.Annotations ;
using UnityEngine ;

namespace Chiyoda.CAD.IO
{
  internal abstract class ObjectSerializer : ISerializer
  {
    private readonly SortedList<string, TypeField> _fields ;

    protected ObjectSerializer( Type type )
    {
      _fields = TypeManager.GetFields( type ).ToSortedList( field => field.Name ) ;
    }
    
    public abstract bool CanRefer { get ; }

    public bool Write( SerializationContext con, in object obj )
    {
      if ( null == obj ) return true ;

      foreach ( var (_, field) in _fields ) {
        var value = field.GetValue( obj ) ;
        if ( value is null ) continue ;
        if ( ! con.WriteField( field, value ) ) return false ;
      }

      return true ;
    }

    public bool Read( DeserializationContext con, Action<object> onRead )
    {
      var obj = CreateInstance( con ) ;
      var result = ReadInto( con, obj ) ;
      onRead( obj ) ;
      return result ;
    }

    public bool ReadInto( DeserializationContext con, in object obj )
    {
      if ( null == obj ) return false ;

      var unusedNames = new HashSet<string>( _fields.Keys ) ;
      using ( con.PushParent( obj ) ) {
        while ( con.GetNextElementName( out var name ) ) {
          if ( ! _fields.TryGetValue( name, out var field ) ) return false ;
          if ( ! con.ReadField( obj, field ) ) return false ;

          unusedNames.Remove( name ) ;
        }

        // 非シリアライズ値を全てnullに
        foreach ( var name in unusedNames ) {
          _fields[ name ].SetValue( obj, null ) ;
        }
      }

      OnAfterRead( con, obj ) ;

      return true ;
    }

    protected abstract object CreateInstance( DeserializationContext con ) ;

    protected abstract void OnAfterRead( DeserializationContext con, in object obj ) ;
  }
  
  internal class ValueTypeSerializer : ObjectSerializer
  {
    private readonly Type _type ;

    public ValueTypeSerializer( Type type ) : base( type )
    {
      _type = type ;
    }

    public override bool CanRefer => false ;

    protected override object CreateInstance( DeserializationContext con )
    {
      return _type.GetDefaultValue() ;
    }

    protected override void OnAfterRead( DeserializationContext con, in object obj )
    {
    }
  }

  internal class ReferenceTypeSerializer : ObjectSerializer
  {
    private readonly Func<IElement, object> _constructor ;
    private readonly Action<IElement, object> _afterReadInto ;

    public ReferenceTypeSerializer( Type type ) : base( type )
    {
      ( _constructor, _afterReadInto ) = TypeManager.GetInstantiator( type ) ;
    }

    public override bool CanRefer => true ;

    protected override object CreateInstance( DeserializationContext con )
    {
      return _constructor( (IElement) con.Parent ) ;
    }

    protected override void OnAfterRead( DeserializationContext con, in object obj )
    {
      _afterReadInto?.Invoke( (IElement) con.Parent, obj ) ;
    }
  }



  #region Primitive Serializers
  
  internal abstract class PrimitiveSerializer<T> : SimpleSerializer<T>
  {
    public sealed override bool CanRefer => false ;

    protected override bool Write( SerializationContext con, in T obj )
    {
      con.WriteString( ToString( obj ) ) ;
      return true ;
    }

    protected override bool Read( DeserializationContext con, out T obj )
    {
      return Convert( con.ReadString(), out obj ) ;
    }

    protected virtual string ToString( in T obj ) => obj.ToString() ;


    protected abstract bool Convert( string str, out T obj ) ;
  }

  internal class NullSerializer : ISerializer
  {
    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      return true ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      onRead( null ) ;
      return true ;
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }
  }
  internal class BooleanSerializer : PrimitiveSerializer<bool>
  {
    protected override string ToString( in bool obj )
    {
      return obj ? "true" : "false" ;
    }

    protected override bool Convert( string str, out bool obj )
    {
      switch ( str.ToLowerInvariant() ) {
        case "0" :
        case "f" :
        case "false" :
        case "no" :
          obj = false ;
          return true ;

        case "1" :
        case "t" :
        case "true" :
        case "yes" :
          obj = true ;
          return true ;

        default:
          obj = false ;
          return false ;
      }
    }
  }
  internal class ByteSerializer : PrimitiveSerializer<byte> { protected override bool Convert( string str, out byte obj ) => byte.TryParse( str, out obj ) ; }
  internal class SByteSerializer : PrimitiveSerializer<sbyte> { protected override bool Convert( string str, out sbyte obj ) => sbyte.TryParse( str, out obj ) ; }
  internal class ShortSerializer : PrimitiveSerializer<short> { protected override bool Convert( string str, out short obj ) => short.TryParse( str, out obj ) ; }
  internal class UShortSerializer : PrimitiveSerializer<ushort> { protected override bool Convert( string str, out ushort obj ) => ushort.TryParse( str, out obj ) ; }
  internal class IntSerializer : PrimitiveSerializer<int> { protected override bool Convert( string str, out int obj ) => int.TryParse( str, out obj ) ; }
  internal class UIntSerializer : PrimitiveSerializer<uint> { protected override bool Convert( string str, out uint obj ) => uint.TryParse( str, out obj ) ; }
  internal class LongSerializer : PrimitiveSerializer<long> { protected override bool Convert( string str, out long obj ) => long.TryParse( str, out obj ) ; }
  internal class ULongSerializer : PrimitiveSerializer<ulong> { protected override bool Convert( string str, out ulong obj ) => ulong.TryParse( str, out obj ) ; }
  internal class FloatSerializer : PrimitiveSerializer<float> { protected override bool Convert( string str, out float obj ) => float.TryParse( str, out obj ) ; }
  internal class DoubleSerializer : PrimitiveSerializer<double> { protected override bool Convert( string str, out double obj ) => double.TryParse( str, out obj ) ; }
  internal class CharSerializer : PrimitiveSerializer<char>
  {
    protected override bool Convert( string str, out char obj )
    {
      if ( string.IsNullOrEmpty( str ) ) {
        obj = default ;
        return false ;
      }
      obj = str[ 0 ] ;
      return ( 1 == str.Length ) ;
    }
  }
  internal class IntPtrSerializer : PrimitiveSerializer<IntPtr>
  {
    protected override bool Convert( string str, out IntPtr obj )
    {
      var result = long.TryParse( str, out var lng ) ;
      if ( ! result ) {
        obj = IntPtr.Zero ;
        return false ;
      }
      obj = new IntPtr( lng ) ;
      return true ;
    }
  }
  internal class UIntPtrSerializer : PrimitiveSerializer<UIntPtr>
  {
    protected override bool Convert( string str, out UIntPtr obj )
    {
      var result = ulong.TryParse( str, out var ulng ) ;
      if ( ! result ) {
        obj = UIntPtr.Zero ;
        return false ;
      }
      obj = new UIntPtr( ulng ) ;
      return true ;
    }
  }
  internal class StringSerializer : PrimitiveSerializer<string>
  {
    protected override string ToString( in string obj ) => obj ;

    protected override bool Convert( string str, out string obj )
    {
      obj = str ;
      return true ;
    }
  }

  internal class EnumSerializer : ISerializer
  {
    private readonly Type _type ;
    public EnumSerializer( Type type )
    {
      _type = type ;
    }

    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      con.WriteString( obj.ToString() ) ;
      return true ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      try {
        onRead( Enum.Parse( _type, con.ReadString(), true ) ) ;
        return true ;
      }
      catch ( Exception ) {
        return false ;
      }
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }
  }

  internal class Vector2Serializer : PrimitiveSerializer<Vector2>
  {
    protected override string ToString( in Vector2 obj )
    {
      return $"{obj.x.ToString( CultureInfo.InvariantCulture )}, {obj.y.ToString( CultureInfo.InvariantCulture )}" ;
    }

    protected override bool Convert( string str, out Vector2 obj )
    {
      obj = default ;

      var array = str.Split( ',' ) ;
      if ( 2 != array.Length ) return false ;

      if ( false == float.TryParse( array[ 0 ].Trim(), out var x ) ) return false ;
      if ( false == float.TryParse( array[ 1 ].Trim(), out var y ) ) return false ;

      obj = new Vector2( x, y ) ;

      return true ;
    }
  }
  internal class Vector3Serializer : PrimitiveSerializer<Vector3>
  {
    protected override string ToString( in Vector3 obj )
    {
      return $"{obj.x.ToString( CultureInfo.InvariantCulture )}, {obj.y.ToString( CultureInfo.InvariantCulture )}, {obj.z.ToString( CultureInfo.InvariantCulture )}" ;
    }

    protected override bool Convert( string str, out Vector3 obj )
    {
      obj = default ;

      var array = str.Split( ',' ) ;
      if ( 3 != array.Length ) return false ;

      if ( false == float.TryParse( array[ 0 ].Trim(), out var x ) ) return false ;
      if ( false == float.TryParse( array[ 1 ].Trim(), out var y ) ) return false ;
      if ( false == float.TryParse( array[ 2 ].Trim(), out var z ) ) return false ;

      obj = new Vector3( x, y, z ) ;

      return true ;
    }
  }
  internal class Vector3dSerializer : PrimitiveSerializer<Vector3d>
  {
    protected override string ToString( in Vector3d obj )
    {
      return $"{obj.x.ToString( CultureInfo.InvariantCulture )}, {obj.y.ToString( CultureInfo.InvariantCulture )}, {obj.z.ToString( CultureInfo.InvariantCulture )}" ;
    }

    protected override bool Convert( string str, out Vector3d obj )
    {
      obj = default ;

      var array = str.Split( ',' ) ;
      if ( 3 != array.Length ) return false ;

      if ( false == double.TryParse( array[ 0 ].Trim(), out var x ) ) return false ;
      if ( false == double.TryParse( array[ 1 ].Trim(), out var y ) ) return false ;
      if ( false == double.TryParse( array[ 2 ].Trim(), out var z ) ) return false ;

      obj = new Vector3d( x, y, z ) ;

      return true ;
    }
  }
  internal class CodSysSerializer : ISerializer
  {
    bool ISerializer.CanRefer => false ;
    
    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      var cod = (LocalCodSys3d) obj ;
      if ( ! con.WriteValue( "origin", cod.Origin ) ) return false ;
      if ( ! con.WriteValue( "x", cod.DirectionX ) ) return false ;
      if ( ! con.WriteValue( "y", cod.DirectionY ) ) return false ;
      if ( ! con.WriteValue( "z", cod.DirectionZ ) ) return false ;
      return true ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      var readCounter = new AllItemReader( array => onRead( new LocalCodSys3d( (Vector3d) array[ 0 ], (Vector3d) array[ 1 ], (Vector3d) array[ 2 ], (Vector3d) array[ 3 ] ) ) ) ;
      if ( ! con.ReadValue( "origin", typeof( Vector3d ), readCounter.ReaderAt( 0 ) ) ) return false ;
      if ( ! con.ReadValue( "x", typeof( Vector3d ), readCounter.ReaderAt( 1 ) ) ) return false ;
      if ( ! con.ReadValue( "y", typeof( Vector3d ), readCounter.ReaderAt( 2 ) ) ) return false ;
      if ( ! con.ReadValue( "z", typeof( Vector3d ), readCounter.ReaderAt( 3 ) ) ) return false ;
      readCounter.End() ;

      return true ;
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }
  }
  internal class DiameterSerializer : PrimitiveSerializer<Diameter>
  {
    protected override string ToString( in Diameter obj )
    {
      return $"{obj.NpsMm.ToString( CultureInfo.InvariantCulture )}/{obj.NpsInch.ToString( CultureInfo.InvariantCulture )}" ;
    }

    protected override bool Convert( string str, out Diameter obj )
    {
      obj = default ;

      var array = str.Split( '/' ) ;
      if ( 2 != array.Length ) return false ;

      if ( false == double.TryParse( array[ 0 ].Trim(), out var mm ) ) return false ;
      if ( false == double.TryParse( array[ 1 ].Trim(), out var inch ) ) return false ;

      obj = new Diameter( (int)mm, inch );

      return true ;
    }
  }
  internal class DiameterRangeSerializer : PrimitiveSerializer<DiameterRange>
  {
    protected override string ToString( in DiameterRange obj )
    {
      return $"{obj.MinDiameterNpsMm}, {obj.MaxDiameterNpsMm}" ;
    }

    protected override bool Convert( string str, out DiameterRange obj )
    {
      obj = default ;

      var array = str.Split( ',' ) ;
      if ( 2 != array.Length ) return false ;

      if ( false == int.TryParse( array[ 0 ].Trim(), out var min ) ) return false ;
      if ( false == int.TryParse( array[ 1 ].Trim(), out var max ) ) return false ;

      obj = new DiameterRange( min, max );

      return true ;
    }
  }

  internal class PairSerializer : ISerializer
  {
    private readonly Type _keyType ;
    private readonly Type _valueType ;
    private readonly Func<object, object> _keyGetter ;
    private readonly Func<object, object> _valueGetter ;
    private readonly Func<object, object, object> _constructor ;

    public PairSerializer( Type pairType )
    {
      _keyType = pairType.GenericTypeArguments[0] ;
      _valueType = pairType.GenericTypeArguments[1] ;
      _keyGetter = CreateGetter( pairType, _keyType, "Key" ) ;
      _valueGetter = CreateGetter( pairType, _valueType, "Value" ) ;
      _constructor = CreateConstructor( pairType ) ;
    }

    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      if ( ! con.WriteValue( "key", _keyGetter( obj ) ) ) return false ;
      if ( ! con.WriteValue( "value", _valueGetter( obj ) ) ) return false ;

      return true ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      var readCounter = new AllItemReader( array => onRead( _constructor( array[ 0 ], array[ 1 ] ) ) ) ;
      if ( ! con.ReadValue( "key", _keyType, readCounter.ReaderAt( 0 ) ) ) return false ;
      if ( ! con.ReadValue( "value", _valueType, readCounter.ReaderAt( 1 ) ) ) return false ;
      readCounter.End() ;
      return true ;
    }
    
    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }

    private static Func<object, object, object> CreateConstructor( Type pairType )
    {
      var ctor = pairType.GetConstructor( pairType.GenericTypeArguments ) ;
      if ( null == ctor ) throw new InvalidOperationException() ;
      
      var keyParam = Expression.Parameter( typeof( object ), "key" ) ;
      var valueParam = Expression.Parameter( typeof( object ), "value" ) ;
      return Expression.Lambda<Func<object, object, object>>(
        Expression.Convert(
          Expression.New( ctor, Expression.Convert( keyParam, pairType.GenericTypeArguments[0] ), Expression.Convert( valueParam, pairType.GenericTypeArguments[1] ) ),
          typeof( object )
        ), keyParam, valueParam
      ).Compile();
    }

    private static Func<object, object> CreateGetter( Type type, Type propType, string propName )
    {
      var prop = type.GetProperty( propName ) ;
      if ( null == prop ) throw new InvalidOperationException() ;

      var thisParam = Expression.Parameter( typeof( object ), "pair" ) ;
      return Expression.Lambda<Func<object, object>>(
        Expression.Convert(
          Expression.Call( Expression.Convert( thisParam, type ), prop.GetGetMethod() ),
          typeof( object )
        ), thisParam
      ).Compile();
    }
  }

  internal class NullableSerializer : ISerializer
  {
    private readonly ISerializer _underlyingSerializer ;

    public NullableSerializer( ISerializer underlyingSerializer )
    {
      _underlyingSerializer = underlyingSerializer ;
    }

    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      if ( obj is null ) return true ;

      return _underlyingSerializer.Write( con, obj ) ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      if ( con.IsEmpty() ) {
        onRead( null ) ;
        return true ;
      }

      return _underlyingSerializer.Read( con, onRead ) ;
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }
  }

  internal class ArraySerializer : ISerializer
  {
    [NotNull]
    private readonly Type _elmType ;
    
    public ArraySerializer( [NotNull] Type elmType )
    {
      _elmType = elmType ;
    }

    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj )
    {
      foreach ( var item in (IEnumerable) obj ) {
        if ( ! con.WriteValue( "item", item ) ) return false ;
      }

      return true ;
    }

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      var readCounter = new AllItemReader( array => onRead( ConvertItemType( array, _elmType ) ) ) ;
      int index = 0 ;
      while ( con.GetNextElementName( out var name ) && name == "item" ) {
        if ( ! con.ReadValue( "item", _elmType, readCounter.ReaderAt( index ) ) ) return false ;
        ++index ;
      }

      readCounter.End() ;

      return true ;
    }

    public bool ReadInto( DeserializationContext con, in object obj )
    {
      var list = ArrayWrapper.Create( obj ) ;
      list.Clear() ;

      var readCounter = new AllItemReader( array =>
      {
        foreach ( var item in array ) list.Add( item ) ;
      } ) ;

      int index = 0 ;
      while ( con.ReadValue( "item", _elmType, readCounter.ReaderAt( index ) ) ) {
        ++index ;
      }
      readCounter.End() ;

      return true ;
    }

    private static object ConvertItemType( object[] array, Type elmType )
    {
      var result = Array.CreateInstance( elmType, array.Length ) ;
      Array.Copy( array, result, array.Length ) ;
      return result ;
    }
  }
  
  #endregion
}
