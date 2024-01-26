using System;
using UnityEngine;
using UnityEngine.UI;
using Chiyoda.UI.PropertyViewPresenters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Chiyoda.UI.PropertyUI
{
  class PropertyCategoryView : MonoBehaviour
  {
    [SerializeField]
    private Text label;

    [SerializeField]
    private GameObject childrenArea;

    [SerializeField]
    private GameObject labelPrefab;
    [SerializeField]
    private GameObject textPrefab;
    [SerializeField]
    private GameObject lengthPrefab;
    [SerializeField]
    private GameObject positionPrefab;
    [SerializeField]
    private GameObject position2DPrefab;
    [SerializeField]
    private GameObject rotaionPrefab;
    [SerializeField]
    private GameObject numberPrefab;
    [SerializeField]
    private GameObject integerPrefab;
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject checkboxPrefab;
    [SerializeField]
    private GameObject listPrefab;
    [SerializeField]
    private GameObject selectPrefab;
    [SerializeField]
    private GameObject diameterRangePrefab;

    private PropertyItemList _propertyItems;

    public string Name
    {
      get { return label.text; }
      set { label.text = value; }
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



    public PropertyItemView CreateItemView( IPropertyViewPresenter presenter, IObjectValueMapper mapper )
    {
      var prefab = GetPrefab( mapper.ValueType );
      if ( null == prefab ) return null;

      var view = Instantiate( prefab ).GetComponent<PropertyItemView>();
      view.CategoryView = this;
      view.SetupPresenter( presenter, mapper );

      return view;
    }

    public void Remove()
    {
      if ( null != _propertyItems ) {
        foreach ( var item in _propertyItems.ToArray() ) {
          Destroy( item.gameObject );
        }
      }
      Destroy( gameObject );
    }

    private GameObject GetPrefab( ValueType valueType )
    {
      switch ( valueType ) {
        case ValueType.Label: return labelPrefab;
        case ValueType.Text: return textPrefab;
        case ValueType.Length: return lengthPrefab;
        case ValueType.Position: return positionPrefab;
        case ValueType.Position2D: return position2DPrefab;
        case ValueType.Rotation: return rotaionPrefab;
        case ValueType.GeneralNumeric: return numberPrefab;
        case ValueType.GeneralInteger: return integerPrefab;
        case ValueType.Button: return buttonPrefab;
        case ValueType.CheckBox: return checkboxPrefab;
        case ValueType.Composite: return listPrefab;
        case ValueType.Select: return selectPrefab;
        case ValueType.DiameterRange: return diameterRangePrefab;
        default: return null;
      }
    }
  }
}
