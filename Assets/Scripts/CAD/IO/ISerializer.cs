using System ;
using System.Collections.Generic ;
using System.Reflection ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.IO
{
  public interface ISerializer
  {
    bool CanRefer { get ; }
    bool Write( SerializationContext con, in object obj ) ;
    bool Read( DeserializationContext con, Action<object> onRead ) ;
    bool ReadInto( DeserializationContext con, in object obj ) ;
  }

  public abstract class AbstractSerializer<T> : ISerializer
  {
    public abstract bool CanRefer { get ; }

    bool ISerializer.Write( SerializationContext con, in object obj ) => Write( con, (T) obj ) ;

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      return Read( con, t => onRead( t ) ) ;
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj )
    {
      throw new NotSupportedException() ;
    }

    protected abstract bool Write( SerializationContext con, in T obj ) ;
    protected abstract bool Read( DeserializationContext con, Action<T> onRead ) ;
  }
  public abstract class AbstractReadIntoSerializer<T> : ISerializer where T : class
  {
    bool ISerializer.CanRefer => false ;

    bool ISerializer.Write( SerializationContext con, in object obj ) => Write( con, (T) obj ) ;

    bool ISerializer.Read( DeserializationContext con, Action<object> onRead )
    {
      throw new NotSupportedException() ;
    }

    bool ISerializer.ReadInto( DeserializationContext con, in object obj ) => ReadInto( con, (T) obj ) ;

    protected abstract bool Write( SerializationContext con, T obj ) ;
    protected abstract bool ReadInto( DeserializationContext con, T obj ) ;
  }

  public abstract class SimpleSerializer<T> : AbstractSerializer<T>
  {
    protected sealed override bool Read( DeserializationContext con, Action<T> onRead )
    {
      if ( false == Read( con, out var t ) ) return false ;

      onRead( t ) ;
      return true ;

    }

    protected abstract bool Read( DeserializationContext con, out T obj ) ;
  }

  internal class Serializers
  {
    private static readonly ISerializer _nullSerializer = new NullSerializer() ;

    private static readonly Dictionary<Type, ISerializer> _serializers = new Dictionary<Type, ISerializer>
    {
      { typeof( bool ), new BooleanSerializer() },
      { typeof( char ), new CharSerializer() },
      { typeof( byte ), new ByteSerializer() },
      { typeof( sbyte ), new SByteSerializer() },
      { typeof( short ), new ShortSerializer() },
      { typeof( ushort ), new UShortSerializer() },
      { typeof( int ), new IntSerializer() },
      { typeof( uint ), new UIntSerializer() },
      { typeof( IntPtr ), new IntPtrSerializer() },
      { typeof( UIntPtr ), new UIntPtrSerializer() },
      { typeof( long ), new LongSerializer() },
      { typeof( ulong ), new ULongSerializer() },
      { typeof( float ), new FloatSerializer() },
      { typeof( double ), new DoubleSerializer() },
      { typeof( string ), new StringSerializer() },
      { typeof( LocalCodSys3d ), new CodSysSerializer() },
      { typeof( Vector2 ), new Vector2Serializer() },
      { typeof( Vector3 ), new Vector3Serializer() },
      { typeof( Vector3d ), new Vector3dSerializer() },
      { typeof( Diameter ), new DiameterSerializer() },
      { typeof( DiameterRange ), new DiameterRangeSerializer() },
    } ;

    private static readonly Dictionary<Type, VersionedValue<ISerializer>> _versionedSerializer = new Dictionary<Type, VersionedValue<ISerializer>>() ;

    public static ISerializer GetSerializer( Type t, int version )
    {
      if ( null == t ) return _nullSerializer ;

      if ( _serializers.TryGetValue( t, out var ser ) ) return ser ;

      if ( t.IsPrimitive ) {
        throw new NotSupportedException() ;
      }

      if ( t.IsEnum ) {
        ser = new EnumSerializer( t ) ;
      }
      else if ( t.IsConstructedGenericType ) {
        if ( typeof( KeyValuePair<,> ) == t.GetGenericTypeDefinition() ) {
          ser = new PairSerializer( t ) ;
        }
        else if ( typeof( Nullable<> ) == t.GetGenericTypeDefinition() ) {
          ser = new NullableSerializer( GetSerializer( Nullable.GetUnderlyingType( t ), version ) ) ;
        }
      }

      if ( null == ser ) {
        ser = FindSerializer( t, version ) ;
        if ( null == ser ) {
          var elmType = t.GetEnumerableItemType() ;
          if ( null != elmType ) {
            ser = new ArraySerializer( elmType ) ;
          }
          else {
            ser = ( t.IsValueType ? (ISerializer) new ValueTypeSerializer( t ) : new ReferenceTypeSerializer( t ) ) ;
          }
        }
      }

      _serializers.Add( t, ser ) ;
      return ser ;
    }

    private static ISerializer FindSerializer( Type type, int version )
    {
      CustomSerializerAttribute maxAttr = null ;
      foreach ( var attr in type.GetCustomAttributes<CustomSerializerAttribute>() ) {
        if ( version < attr.Version ) continue ;

        if ( null == maxAttr || maxAttr.Version < attr.Version ) {
          maxAttr = attr ;
        }
      }

      return maxAttr?.Serializer ;
    }
  }
}