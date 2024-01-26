using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chiyoda
{
  public class IndexableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
  {
    private readonly Dictionary<TKey, TValue> _dic;
    private TKey[] _indexedList = null;

    public IndexableDictionary()
    {
      _dic = new Dictionary<TKey, TValue>();
    }
    public IndexableDictionary( IEqualityComparer<TKey> comparer )
    {
      _dic = new Dictionary<TKey, TValue>( comparer );
    }

    public TValue this[TKey key] { get => _dic[key]; set => _dic[key] = value; }

    public ICollection<TKey> Keys => _dic.Keys;

    public ICollection<TValue> Values => _dic.Values;

    public int Count => _dic.Count;

    public bool IsReadOnly => false;

    public void Add( TKey key, TValue value )
    {
      _dic.Add( key, value );
      ResetList();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add( KeyValuePair<TKey, TValue> item )
    {
      ((ICollection<KeyValuePair<TKey, TValue>>)_dic).Add( item );
      ResetList();
    }

    public void Clear()
    {
      _dic.Clear();
      ResetList();
    }

    public bool Contains( KeyValuePair<TKey, TValue> item ) => ((ICollection<KeyValuePair<TKey, TValue>>)_dic).Contains( item );

    public bool ContainsKey( TKey key ) => _dic.ContainsKey( key );

    public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex ) => ((ICollection<KeyValuePair<TKey, TValue>>)_dic).CopyTo( array, arrayIndex );

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dic.GetEnumerator();

    public bool Remove( TKey key )
    {
      if ( _dic.Remove( key ) ) {
        ResetList();
        return true;
      }
      return false;
    }

    public bool Remove( KeyValuePair<TKey, TValue> item )
    {
      if ( ((ICollection<KeyValuePair<TKey, TValue>>)_dic).Remove( item ) ) {
        ResetList();
        return true;
      }
      return false;
    }

    public bool TryGetValue( TKey key, out TValue value ) => _dic.TryGetValue( key, out value );

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TValue this[int index]
    {
      get
      {
        if ( null == _indexedList ) _indexedList = CreateIndexedList();
        return _dic[_indexedList[index]];
      }
    }

    private TKey[] CreateIndexedList()
    {
      return _dic.Keys.ToArray();
    }

    private void ResetList()
    {
      _indexedList = null;
    }
  }
}
