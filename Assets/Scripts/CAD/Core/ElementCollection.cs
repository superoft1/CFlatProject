using System ;
using System.Collections.Generic ;

namespace Chiyoda.CAD.Core
{
  public class ElementCollection<T> : ICollection<T>, IO.ISerializableList where T : class, IElement
  {
    private readonly MementoSet<T> _items;
    
    public event EventHandler<ItemChangedEventArgs<T>> AfterNewlyItemChanged
    {
      add => _items.AfterNewlyItemChanged += value;
      remove => _items.AfterNewlyItemChanged -= value;
    }
    public event EventHandler<ItemChangedEventArgs<T>> AfterHistoricallyItemChanged
    {
      add => _items.AfterHistoricallyItemChanged += value;
      remove => _items.AfterHistoricallyItemChanged -= value;
    }

    public ElementCollection( IElement elm )
    {
      _items = new MementoSet<T>( elm );
    }

    public void CopyFrom( ElementCollection<T> another, CopyObjectStorage storage )
    {
      _items.SetCopyObjectOrCloneFrom( another._items, storage );
    }

    Type IO.ISerializableList.ItemType => typeof( T ) ;

    public int Count => _items.Count;

    public bool IsReadOnly => _items.IsReadOnly;

    public void Add( T item )
    {
      ((ICollection<T>)_items).Add( item );
    }

    void IO.ISerializableList.Add( object item )
    {
      Add( (T) item ) ;
    }

    public void Clear()
    {
      _items.Clear();
    }

    public bool Contains( T item )
    {
      return _items.Contains( item );
    }

    public void CopyTo( T[] array, int arrayIndex )
    {
      _items.CopyTo( array, arrayIndex );
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _items.GetEnumerator();
    }

    public bool Remove( T item )
    {
      return _items.Remove( item );
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _items.GetEnumerator();
    }
  }
}