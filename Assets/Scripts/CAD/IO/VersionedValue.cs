using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace Chiyoda.CAD.IO
{
  internal class VersionedValue<T>
  {
    private bool _hasDefault = false ;
    private T _default ;

    private List<(int, T)> _versionList ;

    public bool IsEnabled( int version )
    {
      if ( _hasDefault ) return true ;

      if ( null == _versionList ) return false ;

      if ( version < _versionList[ 0 ].Item1 ) return false ;

      return true ;
    }

    public T GetVersionOrDefault( int version )
    {
      TryGetVersion( version, out var value ) ;
      return value ;
    }

    public T DefaultValue
    {
      get => _hasDefault ? _default : default ;
      set
      {
        _default = value ;
        _hasDefault = true ;
      }
    }

    public T this[ int version ]
    {
      get
      {
        if ( TryGetVersion( version, out var value ) ) return value ;
        throw new InvalidOperationException( $"Version {version} is not found and DefaultValue is missing." ) ;
      }
      set
      {
        if ( null == _versionList ) {
          _versionList = new List<(int, T)> { ( version, value ) } ;
        }
        else {
          var index = _versionList.BinarySearch( ( version, default ), VersionComparer.Instance ) ;
          if ( 0 <= index ) {
            _versionList[ index ] = ( version, value ) ;
          }
          else {
            index = ~index ;
            if ( index == _versionList.Count ) _versionList.Add( ( version, value ) ) ;
            else _versionList.Insert( index, ( version, value ) ) ;
          }
        }
      }
    }

    private bool TryGetVersion( int version, out T value )
    {
      if ( null == _versionList ) {
        if ( false == _hasDefault ) {
          value = default ;
          return false ;
        }

        value = _default ; // デフォルト・バージョン
        return true ;
      }

      var index = _versionList.BinarySearch( ( version, default ), VersionComparer.Instance ) ;
      if ( 0 <= index ) {
        value = _versionList[ index ].Item2 ; // 指定バージョンで定義されたものを用いる
        return true ;
      }

      index = ( ~index ) - 1 ;
      if ( 0 <= index ) {
        value = _versionList[ index ].Item2 ; // 直前のバージョンで定義されたものを用いる
        return true ;
      }

      if ( false == _hasDefault ) {
        value = default ;
        return false ;
      }

      value = _default ; // デフォルト・バージョン
      return true ;
    }

    public VersionedValue()
    {
    }

    public VersionedValue( in T defaultValue )
    {
      DefaultValue = defaultValue ;
    }


    
    private class VersionComparer : IComparer<(int, T)>
    {
      public static IComparer<(int, T)> Instance { get ; } = new VersionComparer() ;
      
      public int Compare( (int, T) x, (int, T) y )
      {
        return x.Item1 - y.Item1 ;
      }

      private VersionComparer()
      {
      }
    }
  }
}