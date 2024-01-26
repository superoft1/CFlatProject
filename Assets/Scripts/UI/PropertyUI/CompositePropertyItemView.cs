using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.UI.PropertyViewPresenters;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class CompositePropertyItemView : PropertyItemView
  {
    [SerializeField]
    private InputField count;

    [SerializeField]
    private GameObject childrenArea;

    private object _list;
    private bool _isReadOnly;
    IPropertyViewPresenter _subPresenter;
    private IPropertyItemList _propertyItems;

    protected override void OnPresenterSet()
    {
      if ( ValueType.Composite == ObjectValueMapper.ValueType ) {
        _subPresenter = PropertyViewPresenter.GetSubPresenter( ObjectValueMapper );
      }
      else {
        _subPresenter = null;
      }
    }


    public override object Value
    {
      get { return _list; }
      protected set
      {
        _list = value;
        if ( null == _list || null == _subPresenter ) {
          count.text = "0";
          count.gameObject.SetActive( false );
        }
        else {
          count.text = _subPresenter.PropertyMapperCount.ToString();

          if ( _subPresenter.CanChangePropertyMapperCount ) {
            count.gameObject.SetActive( true );
          }
          else {
            count.gameObject.SetActive( false );
          }
        }

        UpdateAllItems();
      }
    }

    public override bool IsReadOnly
    {
      get { return _isReadOnly; }
      set
      {
        _isReadOnly = value;
        count.readOnly = IsReadOnly;
      }
    }

    public IPropertyItemList PropertyItems
    {
      get
      {
        if ( null == _propertyItems ) {
          _propertyItems = new PropertyItemList( childrenArea );
        }
        return _propertyItems;
      }
    }

    private void Start()
    {
      count.onEndEdit.AddListener( CountChangedListener );
    }
    private void OnDestroy()
    {
      count.onEndEdit.RemoveListener( CountChangedListener );
    }

    private void CountChangedListener( string value )
    {
      int count = ParseInt( value );
      if ( count < 0 ) {
        Value = _list;
        return;
      }

      if ( false == _subPresenter.CanChangePropertyMapperCount ) {
        return;
      }

      _subPresenter.PropertyMapperCount = count;

      UpdateAllItems();

      OnValueChanged( EventArgs.Empty );
    }

    private void UpdateAllItems()
    {
      if( null == _subPresenter ) {
        var array = childrenArea.transform.Cast<Transform>().ToArray();
        childrenArea.transform.DetachChildren();
        foreach ( var t in array ) {
          Destroy( t.gameObject );
        }
        return;
      }

      // 個数の設定
      int newCount = (null == _list) ? 0 : _subPresenter.PropertyMapperCount;
      int oldCount = childrenArea.transform.childCount;

      if ( oldCount < newCount ) {
        var cateogoryView = CategoryView;
        if ( null == cateogoryView ) return;

        for ( int i = oldCount ; i < newCount ; ++i ) {
          var mapper = _subPresenter.GetPropertyMapper( i );
          if ( null == mapper ) throw new ArgumentNullException();

          var itemView = cateogoryView.CreateItemView( _subPresenter, mapper );
          if ( null == itemView ) continue;

          PropertyItems.Add( itemView );
        }
      }
      else if ( oldCount > newCount ) {
        var array = new Transform[oldCount - newCount];
        for ( int i = newCount ; i < oldCount ; ++i ) {
          array[i - newCount] = childrenArea.transform.GetChild( i );
        }
        foreach ( var t in array ) {
          t.SetParent( null );
          Destroy( t.gameObject );
        }
      }

      int minCount = Math.Min( oldCount, newCount );
      for ( int i = 0 ; i < minCount ; ++i ) {
        var mapper = _subPresenter.GetPropertyMapper( i );
        if ( null == mapper ) throw new ArgumentNullException();

        PropertyItems[i].SetupPresenter( _subPresenter, mapper );
      }
    }

    private static int ParseInt( string value )
    {
      int result;
      if ( int.TryParse( value, out result ) ) {
        return result;
      }
      return -1;
    }
  }
}
