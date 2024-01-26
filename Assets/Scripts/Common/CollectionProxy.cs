using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace Chiyoda.Common
{
  /// <summary>
  /// <see cref="System.Collections.Generic.ICollection&lt;T&gt;"/> を <see cref="System.Collections.ICollection"/> のように扱うためのプロキシ・クラスです。
  /// </summary>
  /// <typeparam name="T">対応する <see cref="System.Collections.Generic.ICollection&lt;T&gt;"/> の型パラメータ。</typeparam>
  public class CollectionProxy<T> : ICollectionProxy
  {
    private readonly ICollection<T> _collection ;

    public IEnumerable BaseCollection => _collection ;
    
    public CollectionProxy( ICollection<T> collection )
    {
      _collection = collection ;
    }

    public IEnumerator GetEnumerator()
    {
      return BaseCollection.GetEnumerator() ;
    }

    public void CopyTo( Array array, int index )
    {
      if ( index < 0 || array.Length < index + _collection.Count ) {
        throw new ArgumentOutOfRangeException() ;
      }

      foreach ( var item in _collection ) {
        array.SetValue( item, index ) ;
        ++index ;
      }
    }

    public int Count => _collection.Count ;
    public bool IsSynchronized => false ;
    public object SyncRoot => throw new NotImplementedException() ;
  }
}