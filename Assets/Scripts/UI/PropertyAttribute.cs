using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.UI.PropertyViewPresenters;

namespace Chiyoda.UI
{
  class PropertyAttribute : Attribute
  {
    public PropertyAttribute( PropertyCategory category, string name )
    {
      Category = category;
      Name = name;
      Visibility = PropertyVisibility.Editable;
      IsEditableForBlockPatternChildren = true;
    }

    public int Order { get; set; }

    public PropertyCategory Category { get; private set; }

    public ValueType ValueType { get; set; }

    public string Name { get; private set; }

    public PropertyVisibility Visibility { get; set; }

    public string Label { get; set; }

    public string IsEditablePropertyName { get; set; }

    public string IsVisiblePropertyName { get; set; }

    public string PostSetMethodName { get; set; }

    public string CountPropertyName { get; set; }
    
    public string ListDataMethodName { get ; set ; }

    public string IndexLabelPrefix { get ; set ; }

    public bool IsEditableForBlockPatternChildren { get; set; }

    public ValueType? GetValueType( Type propertyType )
    {
      if ( ValueType.Auto != ValueType ) return ValueType;

      propertyType = GetListElementType( propertyType ) ?? propertyType;

      if ( propertyType.IsEnum ) {
        return ValueType.Select;
      }
      if ( typeof( string ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Text;
      }
      if ( typeof( bool ).IsAssignableFrom( propertyType ) ) {
        return ValueType.CheckBox;
      }
      if ( typeof( float ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Length;
      }
      if ( typeof( double ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Length;
      }
      if ( typeof( int ).IsAssignableFrom( propertyType ) ) {
        return ValueType.GeneralInteger;
      }
      if ( typeof( UnityEngine.Vector3 ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Position;
      }
      if ( typeof( UnityEngine.Vector3d ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Position;
      }
      if ( typeof( UnityEngine.Vector2 ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Position2D;
      }
      if ( typeof( UnityEngine.Quaternion ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Rotation;
      }
      if ( typeof( CAD.Core.IMemorableObject ).IsAssignableFrom( propertyType ) ) {
        return ValueType.Composite;
      }

      return null;
    }

    public static PropertyAttribute Merge( PropertyAttribute getterAttr, PropertyAttribute setterAttr )
    {
      if ( getterAttr.Category != setterAttr.Category ) {
        throw new ArgumentException();
      }
      if ( getterAttr.Name != setterAttr.Name ) {
        throw new ArgumentException();
      }

      return new PropertyAttribute( getterAttr.Category, getterAttr.Name )
      {
        Label = getterAttr.Label ?? setterAttr.Label,
        Order = (0 == getterAttr.Order) ? setterAttr.Order : getterAttr.Order,
        ValueType = (getterAttr.ValueType == ValueType.Auto) ? setterAttr.ValueType : getterAttr.ValueType,
        Visibility = (getterAttr.Visibility == PropertyVisibility.Hidden) ? setterAttr.Visibility : getterAttr.Visibility,
        PostSetMethodName = getterAttr.PostSetMethodName ?? setterAttr.PostSetMethodName,
        CountPropertyName = getterAttr.CountPropertyName ?? setterAttr.CountPropertyName,
        ListDataMethodName = getterAttr.ListDataMethodName ?? setterAttr.ListDataMethodName,
        IndexLabelPrefix = getterAttr.IndexLabelPrefix ?? setterAttr.IndexLabelPrefix,
        IsEditablePropertyName = getterAttr.IsEditablePropertyName ?? setterAttr.IsEditablePropertyName,
        IsEditableForBlockPatternChildren = getterAttr.IsEditableForBlockPatternChildren && setterAttr.IsEditableForBlockPatternChildren,
        IsVisiblePropertyName = getterAttr.IsVisiblePropertyName ?? setterAttr.IsVisiblePropertyName,
      };
    }

    public static Type GetListElementType( Type type )
    {
      if ( type.IsValueType ) return null;

      foreach ( var t in GetAllInterfaces( type ) ) {
        if ( !t.IsGenericType ) continue;

        if ( t.GetGenericTypeDefinition() != typeof( IList<> ) ) continue;

        var args = t.GetGenericArguments();
        if ( 0 < args.Length ) {
          return args[0];
        }
      }

      return null;
    }

    private static IEnumerable<Type> GetAllInterfaces( Type type )
    {
      if ( type.IsInterface ) yield return type;

      foreach ( var t in type.GetInterfaces() ) yield return t;
    }
  }
}
