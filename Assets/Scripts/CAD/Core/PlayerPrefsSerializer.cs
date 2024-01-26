using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.CAD.Core
{
  class PlayerPrefsSerializer
  {
    private readonly Type _targetType;
    private readonly string _storagePrefix;
    private readonly IReaderWriter[] _properties;

    public PlayerPrefsSerializer( Type targetType, string storagePrefix )
    {
      _targetType = targetType;
      _storagePrefix = storagePrefix + ":";

      _properties = _targetType.CollectProperties<object, PlayerPrefsFieldSerializableAttribute>( false ).Select( x => new ReaderWriter( this, x ) ).ToArray();
    }

    public void Load( object data )
    {
      foreach ( var prop in _properties ) {
        prop.Load( data );
      }
    }

    public void Save( object data )
    {
      foreach ( var prop in _properties ) {
        prop.Save( data );
      }
      PlayerPrefs.Save();
    }

    private interface IReaderWriter
    {
      void Load( object data );
      void Save( object data );
    }

    private class ReaderWriter : IReaderWriter
    {
      private readonly string _storagePrefix;
      private readonly PropertyGetterSetterInfo<object, PlayerPrefsFieldSerializableAttribute> _prop;

      public ReaderWriter( PlayerPrefsSerializer serializer, PropertyGetterSetterInfo<object, PlayerPrefsFieldSerializableAttribute> prop )
      {
        _storagePrefix = serializer._storagePrefix;
        _prop = prop;
      }

      public void Load( object data )
      {
        object value = ReadValue( _prop.GetterMethod.ReturnType, _storagePrefix + _prop.PropertyName, _prop.Getter( data ) );
        _prop.Setter( data, value );
      }

      public void Save( object data )
      {
        var value = _prop.Getter( data );
        WriteValue( _prop.GetterMethod.ReturnType, _storagePrefix + _prop.PropertyName, value );
      }

      #region Read/Write

      private static Dictionary<Type, Func<string, object, object>> _readers = new Dictionary<Type, Func<string, object, object>>
      {
        { typeof(bool), ReadBoolean },
        { typeof(short), null }, { typeof(ushort), null },
        { typeof(int), ReadInt32 }, { typeof(uint), ReadUInt32 },
        { typeof(long), null }, { typeof(ulong), null },
        { typeof(float), ReadSingle }, { typeof(double), ReadDouble },
        { typeof(string), ReadString },
      };

      private static Dictionary<Type, Action<string, object>> _writers = new Dictionary<Type, Action<string, object>>
      {
        { typeof(bool), WriteBoolean },
        { typeof(short), null }, { typeof(ushort), null },
        { typeof(int), WriteInt32 }, { typeof(uint), WriteUInt32 },
        { typeof(long), null }, { typeof(ulong), null },
        { typeof(float), WriteSingle }, { typeof(double), WriteDouble },
        { typeof(string), WriteString },
      };

      private static object ReadValue( Type type, string key, object defaultValue )
      {
        if ( type.IsEnum ) {
          return ReadEnum( type, key, defaultValue );
        }

        Func<string, object, object> reader;
        if ( _readers.TryGetValue( type, out reader ) ) {
          if ( null == reader ) {
            throw new InvalidProgramException( type.FullName + " cannot be serialized by PlayerPrefsSerializer." );
          }
          return reader( key, defaultValue );
        }

        throw new InvalidProgramException( type.FullName + " cannot be serialized by PlayerPrefsSerializer." );
      }

      private static void WriteValue( Type type, string key, object value )
      {
        if ( type.IsEnum ) {
          WriteEnum( type, key, value );
          return;
        }

        Action<string, object> writer;
        if ( _writers.TryGetValue( type, out writer ) ) {
          if ( null == writer ) {
            throw new InvalidProgramException( type.FullName + " cannot be serialized by PlayerPrefsSerializer." );
          }
          writer( key, value );
          return;
        }

        throw new InvalidProgramException( type.FullName + " cannot be serialized by PlayerPrefsSerializer." );
      }

      private static object ReadBoolean( string key, object defaultValue )
      {
        return (0 != PlayerPrefs.GetInt( key, (int)defaultValue ));
      }
      private static object ReadInt32( string key, object defaultValue )
      {
        return PlayerPrefs.GetInt( key );
      }
      private static object ReadUInt32( string key, object defaultValue )
      {
        return (uint)PlayerPrefs.GetInt( key, (int)(uint)defaultValue );
      }
      private static object ReadSingle( string key, object defaultValue )
      {
        return PlayerPrefs.GetFloat( key, (float)defaultValue );
      }
      private static object ReadDouble( string key, object defaultValue )
      {
        return (double)PlayerPrefs.GetFloat( key, (float)(double)defaultValue );
      }
      private static object ReadString( string key, object defaultValue )
      {
        return PlayerPrefs.GetString( key, (string)defaultValue );
      }
      private static object ReadEnum( Type type, string key, object defaultValue )
      {
        var value = PlayerPrefs.GetString( key, null );
        if ( null != value && Enum.IsDefined( type, value ) ) {
          return Enum.Parse( type, value );
        }
        else {
          return defaultValue;
        }
      }

      private static void WriteBoolean( string key, object value )
      {
        PlayerPrefs.SetInt( key, (((bool)value) ? 1 : 0) );
      }
      private static void WriteInt32( string key, object value )
      {
        PlayerPrefs.SetInt( key, (int)value );
      }
      private static void WriteUInt32( string key, object value )
      {
        PlayerPrefs.SetInt( key, (int)(uint)value );
      }
      private static void WriteSingle( string key, object value )
      {
        PlayerPrefs.SetFloat( key, (float)value );
      }
      private static void WriteDouble( string key, object value )
      {
        PlayerPrefs.SetFloat( key, (float)(double)value );
      }
      private static void WriteString( string key, object value )
      {
        PlayerPrefs.SetString( key, (string)value );
      }
      private static void WriteEnum( Type type, string key, object value )
      {
        PlayerPrefs.SetString( key, Enum.GetName( type, value ) );
      }

      #endregion
    }
  }
}
