using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class SelectPropertyItemView : PropertyItemView
  {
    private static readonly Dictionary<string, double> NullEnumValues = new Dictionary<string, double>();
    private static readonly Dictionary<Type, Dictionary<string, double>> _enumValuesForType = new Dictionary<Type, Dictionary<string, double>>();

    [SerializeField]
    private Dropdown dropdown;

    private IDictionary<string, double> _listData;
    private IDictionary<string, double> _enumValues;
    private Type _enumType;

    // TODO: 長さ・距離モードの場合は単位つきの数字を用いるように
    public override object Value
    {
      get
      {
        if ( null == _enumValues ) return null;

        if ( dropdown.value < 0 || dropdown.options.Count <= dropdown.value ) return null;

        var name = dropdown.options[dropdown.value].text;

        if ( !_enumValues.TryGetValue( name, out var value ) ) return null;

        if ( null != _listData ) return (int) value ;
        if ( null == _enumType ) return value;

        return Enum.Parse( _enumType, name );
      }
      protected set
      {
        _enumValues = _listData ?? GetSelectionOptions( value, out _enumType ) ;
        var num = GetNumber( value );

        dropdown.options.Clear();
        int i = 0;
        foreach ( var pair in _enumValues ) {
          dropdown.options.Add( new Dropdown.OptionData( pair.Key ) );
          if ( MementoEqualityComparer<double>.Equals( pair.Value, num ) ) dropdown.value = i;
          ++i;
        }
      }
    }

    protected override void SetList( IEnumerable listData )
    {
      if ( null == listData ) {
        _listData = null ;
      }
      else {
        _listData = new Dictionary<string, double>() ;
        int i = 0 ;
        foreach ( var obj in listData ) {
          _listData.Add( obj.ToString(), i ) ;
          ++i ;
        }
      }
    }

    private IDictionary<string, double> GetSelectionOptions( object value, out Type enumType )
    {
      if ( null != Property ) {
        enumType = null;
        return Property.EnumValues ?? NullEnumValues;
      }

      return GetEnumValues( value, out enumType ) ;
    }

    private static IDictionary<string, double> GetEnumValues( object value, out Type enumType )
    {
      enumType = null;
      if ( null == value ) return NullEnumValues;

      enumType = value.GetType();
      if ( !enumType.IsEnum ) return NullEnumValues;

      if ( !_enumValuesForType.TryGetValue( enumType, out var enumValues ) ) {
        enumValues = new Dictionary<string, double>();
        foreach ( var name in Enum.GetNames( enumType ) ) {
          enumValues.Add( name, (int)Enum.Parse( enumType, name ) );
        }
        _enumValuesForType.Add( enumType, enumValues );
      }

      return enumValues;
    }

    private static double GetNumber( object value )
    {
      if ( null == value ) return 0.0;

      if ( value is INamedProperty udprop ) {
        return udprop.Value;
      }

      return Convert.ToDouble( value );
    }

    public override bool IsReadOnly
    {
      get => !dropdown.interactable ;
      set => dropdown.interactable = !value ;
    }

    private void Start()
    {
      dropdown.onValueChanged.AddListener( ValueChangedListener );
    }
    private void OnDestroy()
    {
      dropdown.onValueChanged.RemoveListener( ValueChangedListener );
    }

    private void ValueChangedListener( int newValue )
    {
      OnValueChanged( EventArgs.Empty );
      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }

  }
}
