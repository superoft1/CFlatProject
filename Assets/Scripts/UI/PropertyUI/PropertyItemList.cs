using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.UI.PropertyUI
{
  class PropertyItemList : IPropertyItemList
  {
    private readonly Transform _items;

    public PropertyItemList( GameObject childrenArea )
    {
      _items = childrenArea.transform;
    }

    public PropertyItemView this[int index]
    {
      get { return _items.GetChild( index ).GetComponent<PropertyItemView>(); }
    }

    public int Count
    {
      get { return _items.childCount; }
    }

    public void Add( PropertyItemView item )
    {
      item.transform.SetParent( _items );
    }

    public IEnumerator<PropertyItemView> GetEnumerator()
    {
      foreach ( Component item in _items ) {
        var component = item.GetComponent<PropertyItemView>();
        if ( null != component ) yield return component;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
