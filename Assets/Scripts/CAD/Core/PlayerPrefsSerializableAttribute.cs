using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// PlayerPrefs に保存できるクラスであることを示す属性。
  /// </summary>
  class PlayerPrefsSerializableAttribute : Attribute
  {
    public string StoragePrefix { get; private set; }

    public PlayerPrefsSerializableAttribute( string storagePrefix )
    {
      StoragePrefix = storagePrefix + ":";
    }


    private static readonly Dictionary<Type, PlayerPrefsSerializer> _dic = new Dictionary<Type, PlayerPrefsSerializer>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PlayerPrefsSerializer Get( Type type )
    {
      PlayerPrefsSerializer serializer;
      if ( false == _dic.TryGetValue( type, out serializer ) ) {
        var attr = type.GetCustomAttributes( typeof( PlayerPrefsSerializableAttribute ), true ).FirstOrDefault() as PlayerPrefsSerializableAttribute;
        serializer = new PlayerPrefsSerializer( type, attr.StoragePrefix );
        if ( null == serializer ) {
          throw new InvalidProgramException( type.FullName + " has no PlayerPrefSerializableAttribute." );
        }
        _dic.Add( type, serializer );
      }
      return serializer;
    }
  }
}
