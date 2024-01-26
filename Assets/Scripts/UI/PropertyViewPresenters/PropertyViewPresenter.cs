using System;
using System.CodeDom ;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.UI.PropertyViewPresenters
{
  class PropertyMapperCollection
  {
    private readonly List<IPropertyMapper>[] _mappers = new List<IPropertyMapper>[1 + (int)PropertyCategory.MaxValue];

    public PropertyMapperCollection()
    {
      for ( int i = 0, n = _mappers.Length ; i < n ; ++i ) {
        _mappers[i] = new List<IPropertyMapper>();
      }
    }

    public void AddPropertyMapper( IPropertyMapper mapper )
    {
      int categoryIndex = (int)mapper.Category;
      if ( categoryIndex < 0 || _mappers.Length <= categoryIndex ) throw new ArgumentOutOfRangeException();

      var list = _mappers[categoryIndex];

      int index = list.BinarySearch( mapper, PropertyMapperComparer.Instance );

      if ( index < 0 ) {
        index = ~index;
      }

      if ( index == list.Count ) {
        list.Add( mapper );
      }
      else {
        list.Insert( index, mapper );
      }
    }

    public static PropertyMapperCollection Merge( PropertyMapperCollection[] mappers )
    {
      if ( null == mappers || 0 == mappers.Length ) {
        return new PropertyMapperCollection();
      }
      if ( 1 == mappers.Length ) {
        return mappers[0];
      }

      var result = new PropertyMapperCollection();
      for ( int i = 0, n = result._mappers.Length ; i < n ; ++i ) {
        Merge( result._mappers[i], Array.ConvertAll( mappers, x => x._mappers[i] ) );
      }

      return result;
    }

    private static void Merge( List<IPropertyMapper> result, List<IPropertyMapper>[] mappers )
    {
      result.Clear();

      var n = mappers.Length;
      var dic = mappers[0].ToDictionary( x => x.Name );
      var namesToRemove = new List<string>();

      foreach ( var pair in dic ) {
        for ( int i = 1 ; i < n ; ++i ) {
          if ( false == mappers[i].Exists( pm => IsMergable( pm, pair.Value ) ) ) {
            // 存在しないプロパティは削除
            namesToRemove.Add( pair.Key );
            break;
          }
        }
      }

      // 実際に削除
      foreach ( var name in namesToRemove ) {
        dic.Remove( name );
      }

      // 残ったものをOrder順で登録
      result.AddRange( dic.Values.OrderBy( x => x.Order ) );
    }

    private static bool IsMergable( IPropertyMapper pm1, IPropertyMapper pm2 )
    {
      if ( object.ReferenceEquals( pm1, pm2 ) ) return true;
      if ( pm1.Name != pm2.Name ) return false;

      if ( pm1.UniqueToken != pm2.UniqueToken ) return false;

      return true;
    }

    public int GetAllPropertyMapperCount()
    {
      return _mappers.Sum( list => list.Count );
    }

    public IEnumerable<IPropertyMapper> GetAllPropertyMappers()
    {
      return _mappers.SelectMany( x => x );
    }

    public IPropertyMapper GetPropertyMapper( int index )
    {
      if ( index < 0 ) throw new ArgumentOutOfRangeException( nameof( index ) );

      foreach ( var list in _mappers ) {
        if ( index < list.Count ) return list[index];
      }

      throw new ArgumentOutOfRangeException( nameof( index ) );
    }

    private static IEnumerable<IObjectValueMapper> ToObjectValueMapper( IEnumerable<IPropertyMapper> list, IEnumerable<object> obj )
    {
      var hasBlockPatternChildren = HasBlockPatternChildren( obj );

      foreach ( var item in list ) {
        if ( hasBlockPatternChildren && !item.IsEditableForBlockPatternChildren ) continue;

        yield return item;
      }
    }

    private static bool HasBlockPatternChildren( IEnumerable<object> obj )
    {
      return obj.OfType<IElement>().Any( e => null != e.Ancestor<BlockEdge>() );
    }

    public IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> Filter( object[] objs )
    {
      for ( int i = 0 ; i < _mappers.Length ; ++i ) {
        if ( 0 == _mappers[i].Count ) continue;

        var mappers = ToObjectValueMapper( _mappers[i], objs ).ToArray();
        if ( 0 != mappers.Length ) {
          yield return new KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>( (PropertyCategory)i, mappers );
        }
      }
    }

    private class PropertyMapperComparer : IComparer<IPropertyMapper>
    {
      public static PropertyMapperComparer Instance { get; }

      static PropertyMapperComparer()
      {
        Instance = new PropertyMapperComparer();
      }

      public int Compare( IPropertyMapper x, IPropertyMapper y )
      {
        return (x.Order - y.Order);
      }
    }
  }

  abstract class PropertyViewPresenter : IPropertyViewPresenter
  {
    public static IPropertyViewPresenter Empty { get ; } = new PropertyViewPresenterEmpty() ;

    public static IPropertyViewPresenter CreateForMultiObject( IEnumerable<object> objs )
    {
      return new PropertyViewPresenterForMultiObject( objs );
    }

    public static IPropertyViewPresenter CreateForSingleObject( object obj )
    {
      return new PropertyViewPresenterForSingleObject( obj, null, null );
    }
    private static IPropertyViewPresenter CreateForSingleObject( object obj, IPropertyViewPresenter parentPresenter, IObjectValueMapper parentMapper )
    {
      return new PropertyViewPresenterForSingleObject( obj, parentPresenter, parentMapper );
    }



    public abstract object Target { get; }

    public abstract int PropertyMapperCount { get; set; }

    public virtual bool CanChangePropertyMapperCount { get { return false; } }

    public abstract IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers { get; }

    public abstract IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers { get; }


    public abstract IObjectValueMapper GetPropertyMapper( int index );

    public IPropertyViewPresenter GetSubPresenter( IObjectValueMapper mapper )
    {
      var list = mapper as IListPropertyMapper;
      if ( null != list ) {
        return new PropertyViewPresenterForListProperty( Target, this, list );
      }
      else {
        var obj = GetValue( mapper );
        return CreateForSingleObject( obj, this, mapper );
      }
    }

    public abstract PropertyVisibility GetVisibility( IObjectValueMapper mapper );

    public abstract INamedProperty GetProperty( IObjectValueMapper mapper ) ;

    public abstract object GetValue( IObjectValueMapper mapper );

    public abstract void SetValue( IObjectValueMapper mapper, object newValue );

    public abstract IEnumerable GetValueList( IObjectValueMapper mapper ) ;

    private static ValueType ToValueType( PropertyType type )
    {
      switch ( type ) {
        case PropertyType.Length: return ValueType.Length;
        case PropertyType.Angle: return ValueType.Rotation;
        case PropertyType.GeneralInteger: return ValueType.GeneralInteger;
        case PropertyType.DiameterRange: return ValueType.DiameterRange;
        case PropertyType.Boolean: return ValueType.CheckBox;
        default: return ValueType.GeneralNumeric;
      }
    }



    private class PropertyViewPresenterEmpty : PropertyViewPresenter
    {
      public override object Target { get { return null; } }

      public override int PropertyMapperCount
      {
        get { return 0; }
        set { throw new InvalidOperationException(); }
      }

      public override IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers { get { yield break; } }

      public override IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers { get { return null; } }

      public override IObjectValueMapper GetPropertyMapper( int index )
      {
        throw new ArgumentOutOfRangeException( nameof( index ) );
      }

      public override PropertyVisibility GetVisibility( IObjectValueMapper mapper )
      {
        throw new InvalidOperationException();
      }

      public override INamedProperty GetProperty( IObjectValueMapper mapper )
      {
        throw new InvalidOperationException() ;
      }

      public override object GetValue( IObjectValueMapper mapper )
      {
        throw new InvalidOperationException();
      }

      public override void SetValue( IObjectValueMapper mapper, object newValue )
      {
        throw new InvalidOperationException();
      }
      
      public override IEnumerable GetValueList( IObjectValueMapper mapper )
      {
        throw new InvalidOperationException();
      }
    }

    private class PropertyViewPresenterForSingleObject : PropertyViewPresenter
    {
      private readonly IPropertyViewPresenter _parentPresenter;
      private readonly IObjectValueMapper _parentMapper;
      private readonly PropertyMapperCollection _mappers;
      private readonly object _element;
      private readonly Entity _entity;

      public PropertyViewPresenterForSingleObject( object element, IPropertyViewPresenter parentPresenter, IObjectValueMapper parentMapper )
      {
        _parentPresenter = parentPresenter;
        _parentMapper = parentMapper;

        _element = element;
        _entity = _element as Entity;
        _mappers = CollectMappers( element );
      }

      public override object Target { get { return _element; } }

      public override int PropertyMapperCount
      {
        get
        {
          int userDefinedPropertyCount = (null == _entity) ? 0 : _entity.GetPropertyNameCount();
          return _mappers.GetAllPropertyMapperCount() + userDefinedPropertyCount;
        }
        set { throw new InvalidOperationException(); }
      }

      public override IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers { get { return _mappers.Filter( new[] { _element } ); } }

      public override IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers
      {
        get
        {
          if ( null == _entity || false == _entity.UserDefinedPropertyEditable ) yield break;

          foreach ( var prop in _entity.GetProperties() ) {
            var udprop = prop as IUserDefinedNamedProperty ;
            if ( null == udprop ) continue ;

            if ( udprop is UserDefinedSteppedNamedProperty stprop ) {
              yield return new SteppedNamedPropertyObjectValueMapper( stprop.PropertyName, udprop.Type ) ;
            }
            else if ( null == udprop.EnumValues ) {
              yield return new NamedPropertyObjectValueMapper( prop.PropertyName, udprop.Type ) ;
            }
            else {
              yield return new EnumNamedPropertyObjectValueMapper( prop.PropertyName ) ;
            }
          }
        }
      }

      public override IObjectValueMapper GetPropertyMapper( int index )
      {
        if ( index < 0 ) throw new ArgumentOutOfRangeException( nameof( index ) );

        var count = _mappers.GetAllPropertyMapperCount();
        if ( index < count ) {
          return _mappers.GetPropertyMapper( index );
        }
        if ( null == _entity ) throw new ArgumentOutOfRangeException( nameof( index ) );

        if ( _entity.GetPropertyNameCount() < index ) throw new ArgumentOutOfRangeException( nameof( index ) );

        return new NamedPropertyObjectValueMapper( _entity.GetPropertyNameAt( index ), _entity.GetPropertyTypeAt( index ) );
      }

      public override PropertyVisibility GetVisibility( IObjectValueMapper mapper )
      {
        var visibility = mapper.GetVisibility( _element );
        if ( PropertyVisibility.Editable != visibility ) return visibility;

        if ( null != _parentPresenter ) {
          return _parentPresenter.GetVisibility( _parentMapper );
        }

        return PropertyVisibility.Editable;
      }

      public override INamedProperty GetProperty( IObjectValueMapper mapper )
      {
        return mapper.GetPropertyInfo( _element );
      }

      public override object GetValue( IObjectValueMapper mapper )
      {
        return mapper.GetObjectValue( _element );
      }

      public override void SetValue( IObjectValueMapper mapper, object newValue )
      {
        mapper.SetObjectValue( _element, newValue );
        if ( null != _parentPresenter ) {
          _parentPresenter.SetValue( _parentMapper, _element );
        }
      }

      public override IEnumerable GetValueList( IObjectValueMapper mapper )
      {
        return mapper.GetValueList( _element );
      }
    }

    private class PropertyViewPresenterForMultiObject : PropertyViewPresenter
    {
      private readonly PropertyMapperCollection _mappers;

      private readonly object[] _objs;
      private readonly HashSet<IObjectValueMapper> _multiValuedMappers = new HashSet<IObjectValueMapper>();

      public PropertyViewPresenterForMultiObject( IEnumerable<object> objs )
      {
        _objs = objs.ToArray();
        _mappers = PropertyMapperCollection.Merge( Array.ConvertAll( _objs, CollectMappers ) );
        foreach ( var mapper in _mappers.GetAllPropertyMappers() ) {
          if ( _objs.All( obj => PropertyVisibility.Hidden == mapper.GetVisibility( obj ) ) ) continue;

          if ( HasMultiValues( mapper ) ) {
            _multiValuedMappers.Add( mapper );
          }
        }
      }

      public override object Target { get { return _objs; } }

      private bool HasMultiValues( IObjectValueMapper item )
      {
        object first = null;
        foreach ( var element in _objs ) {
          var value = item.GetObjectValue( element );

          if ( null == first ) {
            first = value;
          }
          else {
            if ( false == object.Equals( first, value ) ) return true;
          }
        }

        return false;
      }

      public override int PropertyMapperCount
      {
        get { return _mappers.GetAllPropertyMapperCount(); }
        set { throw new InvalidOperationException(); }
      }

      public override IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers
      {
        get { return _mappers.Filter( _objs ); }
      }

      public override IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers { get { return null; } }

      public override IObjectValueMapper GetPropertyMapper( int index )
      {
        return _mappers.GetPropertyMapper( index );
      }

      public override PropertyVisibility GetVisibility( IObjectValueMapper mapper )
      {
        if ( _multiValuedMappers.Contains( mapper ) ) return PropertyVisibility.ReadOnly;
        return _objs.Aggregate( PropertyVisibility.Hidden, ( lastVisibility, element ) =>
        {
          if ( PropertyVisibility.ReadOnly == lastVisibility ) return PropertyVisibility.ReadOnly;

          var visibility = mapper.GetVisibility( element );
          if ( PropertyVisibility.Hidden == visibility ) return lastVisibility;
          return visibility;
        } );
      }

      public override INamedProperty GetProperty( IObjectValueMapper mapper )
      {
        if ( 0 == _objs.Length ) return null;

        if ( _multiValuedMappers.Contains( mapper ) ) return null;

        return mapper.GetPropertyInfo( _objs[0] );
      }

      public override object GetValue( IObjectValueMapper mapper )
      {
        if ( 0 == _objs.Length ) return null;

        if ( _multiValuedMappers.Contains( mapper ) ) return null;

        return mapper.GetObjectValue( _objs[0] );
      }

      public override void SetValue( IObjectValueMapper mapper, object newValue )
      {
        if ( _multiValuedMappers.Contains( mapper ) ) return;

        foreach ( var element in _objs ) {
          if ( PropertyVisibility.Editable == mapper.GetVisibility( element ) ) {
            mapper.SetObjectValue( element, newValue );
          }
        }
      }
            
      public override IEnumerable GetValueList( IObjectValueMapper mapper )
      {
        if ( 0 == _objs.Length ) return null;

        if ( _multiValuedMappers.Contains( mapper ) ) return null;

        return mapper.GetValueList( _objs[0] );
      }
    }

    private class PropertyViewPresenterForListProperty : PropertyViewPresenter
    {
      private readonly IPropertyViewPresenter _parentPresenter;
      private readonly IListPropertyMapper _parentMapper;
      private readonly object _owner;

      public PropertyViewPresenterForListProperty( object owner, IPropertyViewPresenter parentPresenter, IListPropertyMapper parentMapper )
      {
        _parentPresenter = parentPresenter;
        _parentMapper = parentMapper;

        _owner = owner;
      }

      public override object Target { get { return _parentMapper.GetObjectValue( _owner ); } }

      public override int PropertyMapperCount
      {
        get { return _parentMapper.GetCount( _owner ); }
        set
        {
          _parentMapper.SetCount( _owner, value );
          if ( null != _parentPresenter ) {
            _parentPresenter.SetValue( _parentMapper, _owner );
          }
        }
      }

      public override bool CanChangePropertyMapperCount { get { return _parentMapper.CanSetCount( _owner ); } }

      public override IEnumerable<KeyValuePair<PropertyCategory, IEnumerable<IObjectValueMapper>>> PropertyMappers { get { yield break; } }

      public override IEnumerable<IObjectValueMapper> UserDefinedPropertyMappers
      {
        get
        {
          int count = _parentMapper.GetCount( _owner );
          for ( int i = 0 ; i < count ; ++i ) {
            yield return _parentMapper.GetElementMapper( i );
          }
        }
      }

      public override IObjectValueMapper GetPropertyMapper( int index )
      {
        return _parentMapper.GetElementMapper( index );
      }

      public override PropertyVisibility GetVisibility( IObjectValueMapper mapper )
      {
        var visibility = mapper.GetVisibility( _owner );
        if ( PropertyVisibility.Editable != visibility ) return visibility;

        if ( null != _parentPresenter ) {
          return _parentPresenter.GetVisibility( _parentMapper );
        }

        return PropertyVisibility.Editable;
      }

      public override INamedProperty GetProperty( IObjectValueMapper mapper )
      {
        return mapper.GetPropertyInfo( _owner );
      }

      public override object GetValue( IObjectValueMapper mapper )
      {
        return mapper.GetObjectValue( _owner );
      }

      public override void SetValue( IObjectValueMapper mapper, object newValue )
      {
        mapper.SetObjectValue( _owner, newValue );
        if ( null != _parentPresenter ) {
          _parentPresenter.SetValue( _parentMapper, _owner );
        }
      }
            
      public override IEnumerable GetValueList( IObjectValueMapper mapper )
      {
        return mapper.GetValueList( _owner );
      }
    }



    private static readonly Dictionary<Type, PropertyMapperCollection> _dicTypeMappers = new Dictionary<Type, PropertyMapperCollection>();

    private static PropertyMapperCollection CollectMappers( object obj )
    {
      var type = obj.GetType();

      PropertyMapperCollection result;
      if ( !_dicTypeMappers.TryGetValue( type, out result ) ) {
        result = new PropertyMapperCollection();
        _dicTypeMappers.Add( type, result );

        foreach ( var mapper in GetPropertyMappers( type ) ) {
          result.AddPropertyMapper( mapper );
        }
      }

      return result;
    }

    private static IEnumerable<IPropertyMapper> GetPropertyMappers( Type type )
    {
      foreach ( var mapper in GetPropertyMappersForProperties( type ) ) {
        yield return mapper;
      }

      foreach ( var mapper in GetPropertyMappersForMethodPairs( type ) ) {
        yield return mapper;
      }

      foreach ( var mapper in GetPropertyMappersForExtensionMethodPairs( type ) ) {
        yield return mapper;
      }
    }

    private class PropertyAttributeEqualityComparer : IEqualityComparer<PropertyAttribute>
    {
      public bool Equals( PropertyAttribute x, PropertyAttribute y )
      {
        return (x.Category == y.Category) && (x.Name == y.Name);
      }

      public int GetHashCode( PropertyAttribute obj )
      {
        return obj.Category.GetHashCode() ^ obj.Name.GetHashCode();
      }
    }

    private static IEnumerable<IPropertyMapper> GetPropertyMappersForProperties( Type type )
    {
      foreach ( var prop in type.CollectProperties<object, PropertyAttribute>( true ) ) {
        var mapper = CreatePropertyMapper( type, prop );
        if ( null != mapper ) {
          yield return mapper;
        }
      }
    }
    private static IEnumerable<IPropertyMapper> GetPropertyMappersForMethodPairs( Type type )
    {
      foreach ( var prop in type.CollectGetterSetterPairs<object, PropertyAttribute>( new PropertyAttributeEqualityComparer(), true ) ) {
        var mapper = CreatePropertyMapper( type, prop );
        if ( null != mapper ) {
          yield return mapper;
        }
      }
      foreach ( var prop in type.CollectActions<object, PropertyAttribute>( true ) ) {
        var mapper = CreateActionPropertyMapper( type, prop );
        if ( null != mapper ) {
          yield return mapper;
        }
      }
    }
    private static IEnumerable<IPropertyMapper> GetPropertyMappersForExtensionMethodPairs( Type type )
    {
      foreach ( var prop in type.CollectGetterSetterPairs<object, PropertyAttribute>( GetExtensionPropertyMethods(), new PropertyAttributeEqualityComparer(), true ) ) {
        var mapper = CreatePropertyMapper( type, prop );
        if ( null != mapper ) {
          yield return mapper;
        }
      }
      foreach ( var prop in type.CollectActions<object, PropertyAttribute>( GetExtensionPropertyMethods(), true ) ) {
        var mapper = CreateActionPropertyMapper( type, prop );
        if ( null != mapper ) {
          yield return mapper;
        }
      }
    }

    private static List<MethodInfo> _extensionPropertyMethods;
    private static IEnumerable<MethodInfo> GetExtensionPropertyMethods()
    {
      if ( null == _extensionPropertyMethods ) {
        _extensionPropertyMethods = new List<MethodInfo>();

        foreach ( var type in Assembly.GetExecutingAssembly().GetTypes() ) {
          if ( !type.IsAbstract || !type.IsSealed ) continue; // static class == abstract sealed class

          foreach ( var method in type.GetMethods( BindingFlags.Static | BindingFlags.Public ) ) {
            var attr = method.GetCustomAttributes( typeof( PropertyAttribute ), false ).FirstOrDefault() as PropertyAttribute;
            if ( null == attr ) continue;

            _extensionPropertyMethods.Add( method );
          }
        }
      }

      return _extensionPropertyMethods;
    }

    private static IPropertyMapper CreatePropertyMapper( Type type, PropertyGetterSetterInfo<object, PropertyAttribute> prop )
    {
      if ( null == prop.GetterMethod ) return null;
      var attr = prop.GetterAttribute;
      if ( prop.SetterAttribute != attr && null != prop.SetterAttribute ) {
        attr = PropertyAttribute.Merge( attr, prop.SetterAttribute );
      }

      if ( PropertyVisibility.Hidden == attr.Visibility ) return null;

      var valueType = attr.GetValueType( prop.GetterMethod.ReturnType );
      if ( !valueType.HasValue ) return null;

      Type elmType = PropertyAttribute.GetListElementType( prop.GetterMethod.ReturnType );

      Func<object, object> getter = prop.Getter;

      Action<object, object> setter = null;
      Func<object, bool> isEditableGetter = null;
      Func<object, bool> isVisibleGetter = null;
      Action<object> postSetter = null;
      if ( (null != prop.SetterMethod && PropertyVisibility.Editable == attr.Visibility) || (null != elmType) ) {
        setter = prop.Setter;

        var thisParam = Expression.Parameter( typeof( object ), "obj" );
        var postSetMethod = GetPostSetMethod( type, attr.PostSetMethodName );
        postSetter = (null == postSetMethod) ? null : Expression.Lambda<Action<object>>(
            Expression.Call( Expression.TypeAs( thisParam, postSetMethod.DeclaringType ), postSetMethod ), thisParam
          ).Compile();
      }
      {
        var thisParam = Expression.Parameter( typeof( object ), "obj" );

        var isEditableGetMethod = GetGetBooleanMethod( type, attr.IsEditablePropertyName );
        isEditableGetter = (null == isEditableGetMethod) ? null : Expression.Lambda<Func<object, bool>>(
          Expression.Call( Expression.TypeAs( thisParam, isEditableGetMethod.DeclaringType ), isEditableGetMethod ), thisParam
        ).Compile();

        var isVisibleGetMethod = GetGetBooleanMethod( type, attr.IsVisiblePropertyName );
        isVisibleGetter = (null == isVisibleGetMethod) ? null : Expression.Lambda<Func<object, bool>>(
          Expression.Call( Expression.TypeAs( thisParam, isVisibleGetMethod.DeclaringType ), isVisibleGetMethod ), thisParam
        ).Compile();
      }

      Func<object, IEnumerable> listGetter = null ;
      if ( ValueType.Select == valueType.Value ) {
        var listDataMethod = GetListDataMethod( type, attr.ListDataMethodName );
        if ( null != listDataMethod ) {
          var thisParam = Expression.Parameter( typeof( object ), "obj" ) ;
          if ( listDataMethod.IsStatic ) {
            listGetter = Expression.Lambda<Func<object, IEnumerable>>(
                Expression.Convert(
                  Expression.Call( listDataMethod ),
                  typeof( IEnumerable )
                ), thisParam
              ).Compile() ;
          }
          else {
            listGetter = Expression.Lambda<Func<object, IEnumerable>>(
                Expression.Convert(
                  Expression.Call( Expression.TypeAs( thisParam, listDataMethod.DeclaringType ), listDataMethod ),
                  typeof( IEnumerable )
                ), thisParam
              ).Compile() ;
          }
        }
      }

      if ( null != elmType ) {
        Func<object, int> countGetter = null;
        Action<object, int> countSetter = null;
        if ( !string.IsNullOrEmpty( attr.CountPropertyName ) ) {
          var thisParam = Expression.Parameter( typeof( object ), "obj" );
          var countProperty = GetCountProperty( type, attr.CountPropertyName );
          if ( null != countProperty ) {
            var getMethod = countProperty.GetGetMethod();
            var setMethod = countProperty.GetSetMethod();
            if ( null != getMethod ) {
              countGetter = Expression.Lambda<Func<object, int>>(
                  Expression.Call( Expression.TypeAs( thisParam, getMethod.DeclaringType ), getMethod ), thisParam
                ).Compile();
            }
            if ( null != setMethod ) {
              var countParam = Expression.Parameter( typeof( int ), "value" );
              countSetter = Expression.Lambda<Action<object, int>>(
                  Expression.Call( Expression.TypeAs( thisParam, setMethod.DeclaringType ), setMethod, countParam ), thisParam, countParam
                ).Compile();
            }
          }
        }
        return new ListPropertyMapper(
                    elmType,
                    attr.Category,
                    attr.Name,
                    attr.Order,
                    attr.Label,
                    attr.IndexLabelPrefix,
                    valueType.Value,
                    attr.IsEditableForBlockPatternChildren,
                    prop.UniqueToken,
                    getter,
                    countGetter,
                    countSetter,
                    postSetter,
                    listGetter );
      }

      if ( ValueType.Composite == valueType.Value ) {
        return new ObjectPropertyMapper(
                    prop.GetterMethod.ReturnType,
                    attr.Category,
                    attr.Name,
                    attr.Order,
                    attr.Label,
                    attr.IsEditableForBlockPatternChildren,
                    prop.UniqueToken,
                    getter,
                    isEditableGetter,
                    isVisibleGetter,
                    postSetter );
      }

      return new PropertyMapper(
                    attr.Category,
                    attr.Name,
                    attr.Order,
                    attr.Label,
                    valueType.Value,
                    attr.IsEditableForBlockPatternChildren,
                    prop.UniqueToken,
                    getter,
                    setter,
                    isEditableGetter,
                    isVisibleGetter,
                    postSetter,
                    listGetter );
    }

    private static PropertyInfo GetCountProperty( Type type, string countPropertyName )
    {
      if ( string.IsNullOrEmpty( countPropertyName ) ) return null;

      return GetProperty( type, countPropertyName, typeof( int ) );
    }

    private static IPropertyMapper CreateActionPropertyMapper( Type type, PropertyGetterSetterInfo<object, PropertyAttribute> prop )
    {
      if ( null == prop.SetterMethod ) return null;
      var attr = prop.SetterAttribute;

      if ( PropertyVisibility.Hidden == attr.Visibility ) return null;

      Action<object, object> setter = prop.Setter;

      Func<object, bool> isEditableGetter = null;
      Func<object, bool> isVisibleGetter = null;
      Action<object> postSetter = null;
      if ( null != prop.SetterMethod && PropertyVisibility.Editable == attr.Visibility ) {
        setter = prop.Setter;

        var thisParam = Expression.Parameter( typeof( object ), "obj" );
        var postSetMethod = GetPostSetMethod( type, attr.PostSetMethodName );
        postSetter = (null == postSetMethod) ? null : Expression.Lambda<Action<object>>(
            Expression.Call( Expression.TypeAs( thisParam, postSetMethod.DeclaringType ), postSetMethod ), thisParam
          ).Compile();
      }
      {
        var thisParam = Expression.Parameter( typeof( object ), "obj" );

        var isEditableGetMethod = GetGetBooleanMethod( type, attr.IsEditablePropertyName );
        isEditableGetter = (null == isEditableGetMethod) ? null : Expression.Lambda<Func<object, bool>>(
          Expression.Call( Expression.TypeAs( thisParam, isEditableGetMethod.DeclaringType ), isEditableGetMethod ), thisParam
        ).Compile();

        var isVisibleGetMethod = GetGetBooleanMethod( type, attr.IsVisiblePropertyName );
        isVisibleGetter = (null == isVisibleGetMethod) ? null : Expression.Lambda<Func<object, bool>>(
          Expression.Call( Expression.TypeAs( thisParam, isVisibleGetMethod.DeclaringType ), isVisibleGetMethod ), thisParam
        ).Compile();
      }

      return new PropertyMapper(
                  attr.Category,
                  attr.Name,
                  attr.Order,
                  attr.Label,
                  ValueType.Button,
                  attr.IsEditableForBlockPatternChildren,
                  prop.UniqueToken,
                  null,
                  setter,
                  isEditableGetter,
                  isVisibleGetter,
                  postSetter,
                  null );
    }

    private static MethodInfo GetPostSetMethod( Type type, string postSetMethodName )
    {
      if ( string.IsNullOrEmpty( postSetMethodName ) ) return null;

      return type.GetMethod( postSetMethodName, Type.EmptyTypes );
    }

    private static MethodInfo GetListDataMethod( Type type, string listDataMethodName )
    {
      if ( string.IsNullOrEmpty( listDataMethodName ) ) return null;

      var method = type.GetMethod( listDataMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                   ?? type.GetProperty( listDataMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )?.GetGetMethod( true ) ;
      if ( null == method ) return null ;

      if ( ! typeof( IEnumerable ).IsAssignableFrom( method.ReturnType ) ) return null ;

      return method ;
    }

    private static MethodInfo GetGetBooleanMethod( Type type, string propName )
    {
      if ( string.IsNullOrEmpty( propName ) ) return null;

      var prop = type.GetProperty( propName, typeof( bool ) );
      if ( null != prop && null != prop.GetGetMethod() ) {
        return prop.GetGetMethod();
      }

      var method = type.GetMethod( propName, Type.EmptyTypes );
      if ( null != method && typeof( bool ) == method.ReturnType ) {
        return method;
      }

      return null;
    }

    private static PropertyInfo GetIndexer( Type type, params Type[] paramTypes )
    {
      return type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
        .FirstOrDefault( prop => prop.GetIndexParameters().Select( pr => pr.ParameterType ).SequenceEqual( paramTypes ) );
    }

    private static PropertyInfo GetProperty( Type type, string name, Type returnType )
    {
      if ( type.IsInterface ) {
        var prop = type.GetProperty( name, returnType );
        if ( null != prop ) return prop;

        foreach ( var interfaceType in type.GetInterfaces() ) {
          var interfaceProp = interfaceType.GetProperty( name, returnType );
          if ( null != interfaceProp ) return interfaceProp;
        }
      }
      else {
        for ( ; typeof( object ) != type ; type = type.BaseType ) {
          var prop = type.GetProperty( name, returnType );
          if ( null != prop ) return prop;
        }
      }

      return null;
    }

    private static T CreateFunction<T>( Type objectType, Func<object, object> propGetter, MethodInfo methodInfo, Type[] paramTypes, Type returnType )
    {
      if ( null == methodInfo ) {
        return default( T );
      }

      var thisParam = Expression.Parameter( typeof( object ), "value" );
      var funcArgs = new ParameterExpression[paramTypes.Length + 1];
      var lambdaArgs = new Expression[paramTypes.Length];

      funcArgs[0] = thisParam;
      var methodParams = methodInfo.GetParameters();
      for ( int i = 0 ; i < paramTypes.Length ; ++i ) {
        var param = Expression.Parameter( paramTypes[i], "arg" + i );
        funcArgs[i + 1] = param;
        lambdaArgs[i] = Expression.Convert( param, methodParams[i].ParameterType );
      }

      Expression targetObject;
      if ( null == propGetter ) {
        targetObject = thisParam;
      }
      else {
        var method = propGetter.GetType().GetMethod( "Invoke" );
        targetObject = Expression.Call( Expression.Constant( propGetter ), method, thisParam );
      }
      Expression ret = Expression.Call( Expression.TypeAs( targetObject, methodInfo.DeclaringType ), methodInfo, lambdaArgs );
      if ( typeof( void ) != returnType ) {
        ret = Expression.Convert( ret, returnType );
      }

      return Expression.Lambda<T>( ret, funcArgs ).Compile();
    }



    private class PropertyMapper : IPropertyMapper
    {
      public long UniqueToken { get; private set; }
      public PropertyCategory Category { get; private set; }
      public ValueType ValueType { get; private set; }
      public string Name { get; private set; }
      public string Label { get; private set; }
      public int Order { get; private set; }
      public bool IsEditableForBlockPatternChildren { get; }

      private readonly Func<object, object> _getter;
      private readonly Action<object, object> _setter;
      private readonly Func<object, bool> _isEditableGetter;
      private readonly Func<object, bool> _isVisibleGetter;
      private readonly Action<object> _postSetter;
      private readonly PropertyVisibility _visibility;
      private readonly Func<object, IEnumerable> _itemDataGetter ;

      public PropertyMapper(
                  PropertyCategory category, string name, int order, string label, ValueType valueType, bool isEditableForBlockPatternChildren,
                  long uniqueToken,
                  Func<object, object> getter,
                  Action<object, object> setter,
                  Func<object, bool> isEditableGetter,
                  Func<object, bool> isVisibleGetter,
                  Action<object> postSetter,
                  Func<object, IEnumerable> itemDataGetter )
      {
        UniqueToken = uniqueToken;
        IsEditableForBlockPatternChildren = isEditableForBlockPatternChildren;

        Category = category;
        ValueType = valueType;
        Name = name;
        Order = order;
        Label = label ?? name;
        _visibility = (null == setter ? PropertyVisibility.ReadOnly : PropertyVisibility.Editable);

        _getter = getter;
        _setter = setter;
        _isEditableGetter = isEditableGetter;
        _isVisibleGetter = isVisibleGetter;
        _postSetter = postSetter;

        if ( ValueType.Select == valueType ) {
          _itemDataGetter = itemDataGetter ;
        }
      }

      public INamedProperty GetPropertyInfo( object obj )
      {
        return null;
      }

      public object GetObjectValue( object obj )
      {
        return _getter?.Invoke( obj );
      }

      public void SetObjectValue( object obj, object value )
      {
        if ( null == _setter ) return;
        _setter( obj, value );

        _postSetter?.Invoke( obj );
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        if ( null != _isVisibleGetter && false == _isVisibleGetter( obj ) ) {
          // 強制非表示
          return PropertyVisibility.Hidden;
        }
        if ( (PropertyVisibility.Editable == _visibility) && (null != _isEditableGetter) && (false == _isEditableGetter( obj )) ) {
          return PropertyVisibility.ReadOnly;
        }
        return _visibility;
      }

      public IEnumerable GetValueList( object obj )
      {
        return _itemDataGetter?.Invoke( obj );
      }
    }

    private class ListPropertyMapper : IListPropertyMapper
    {
      public long UniqueToken { get; private set; }
      public PropertyCategory Category { get; private set; }
      public ValueType ValueType { get { return UI.ValueType.Composite; } }
      public ValueType ElementValueType { get; }
      public string Name { get; }
      public string Label { get; }
      public string IndexLabelPrefix { get; }
      public int Order { get; }
      public bool IsEditableForBlockPatternChildren { get; }

      private readonly Type _elmType;
      private readonly ValueType _elmValueType;
      private readonly Func<object, object> _propGetter;
      private readonly Func<object, bool> _readOnlyGetter;
      private readonly Func<object, int, object> _elementGetter;
      private readonly Action<object, int, object> _elementSetter;
      private readonly Func<object, int> _countGetter;
      private readonly Action<object, int> _countSetter;
      private readonly Action<object> _postSetter;
      private readonly Func<object, IEnumerable> _itemDataGetter ;

      private readonly List<IObjectValueMapper> _elementMappers = new List<IObjectValueMapper>();

      public ListPropertyMapper(
        Type elmType, PropertyCategory category, string name, int order, string label, string indexLabelPrefix, ValueType elmValueType, bool isEditableForBlockPatternChildren,
        long uniqueToken,
        Func<object, object> getter,
        Func<object, int> countGetter,
        Action<object, int> countSetter,
        Action<object> postSetter,
        Func<object, IEnumerable> itemDataGetter )
      {
        IsEditableForBlockPatternChildren = isEditableForBlockPatternChildren;

        _elmType = elmType;
        UniqueToken = uniqueToken;
        Category = category;
        ElementValueType = elmValueType;
        Name = name;
        Order = order;
        Label = label ?? name;
        IndexLabelPrefix = indexLabelPrefix ?? "" ;

        _propGetter = getter;
        _postSetter = postSetter;

        Type listType = typeof( IList<> ).MakeGenericType( elmType );

        _countGetter = countGetter ?? CreateCountGetter( listType, _propGetter );
        _countSetter = countSetter ?? CreateCountSetter( listType, _propGetter );
        _readOnlyGetter = CreateReadOnlyGetter( listType, _propGetter );
        _elementGetter = CreateElementGetter( listType, _propGetter );
        _elementSetter = CreateElementSetter( listType, _propGetter );

        if ( ValueType.Select == elmValueType ) {
          _itemDataGetter = itemDataGetter ;
        }
      }
      
      public INamedProperty GetPropertyInfo( object obj )
      {
        return null ;
      }

      public object GetObjectValue( object obj )
      {
        return _propGetter?.Invoke( obj );
      }

      public void SetObjectValue( object obj, object value )
      {
        // GetObjectValue()経由で扱うため、_postSetterのみ設定
        _postSetter?.Invoke( obj );
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        if ( null == _propGetter ) return PropertyVisibility.Hidden;

        if ( null != _readOnlyGetter && true == _readOnlyGetter( obj ) ) {
          // ReadOnlyコレクションは編集時もReadOnly
          return PropertyVisibility.ReadOnly;
        }

        return PropertyVisibility.Editable;
      }

      public IEnumerable GetValueList( object obj )
      {
        return _itemDataGetter?.Invoke( obj );
      }


      public int GetCount( object obj )
      {
        if ( null == _countGetter ) return 0;
        return _countGetter( obj );
      }

      public void SetCount( object obj, int count )
      {
        if ( null == _countSetter ) return;
        _countSetter( obj, count );

        if ( count < _elementMappers.Count ) {
          _elementMappers.RemoveRange( count, _elementMappers.Count - count );
        }

        _postSetter?.Invoke( obj );
      }

      public bool CanSetCount( object obj )
      {
        if ( null == _countSetter ) return false;
        if ( null != _readOnlyGetter && _readOnlyGetter( obj ) ) return false;

        return true;
      }

      public object GetElement( object obj, int index )
      {
        return _elementGetter( obj, index );
      }

      public void SetElement( object obj, int index, object newValue )
      {
        _elementSetter( obj, index, newValue );
      }

      public IObjectValueMapper GetElementMapper( int index )
      {
        if ( _elementMappers.Count <= index ) {
          _elementMappers.AddRange( Enumerable.Range( _elementMappers.Count, index + 1 ).Select( i => (IObjectValueMapper)new ElementMapper( this, i ) ) );
        }
        return _elementMappers[index];
      }

      #region ElementMapper

      private class ElementMapper : IObjectValueMapper
      {
        private readonly IListPropertyMapper _listPropertyMapper;
        private readonly int _index;

        public ElementMapper( IListPropertyMapper listPropertyMapper, int index )
        {
          _listPropertyMapper = listPropertyMapper;
          _index = index;
        }

        public string Label
        {
          get
          {
            if ( string.IsNullOrEmpty( _listPropertyMapper.IndexLabelPrefix ) ) {
              return ( _index + 1 ).ToString() ;
            }
            else {
              return $"{_listPropertyMapper.IndexLabelPrefix} {_index + 1}" ;
            }
          }
        }

        public ValueType ValueType { get { return _listPropertyMapper.ElementValueType; } }

        public INamedProperty GetPropertyInfo( object obj )
        {
          return null;
        }

        public object GetObjectValue( object obj )
        {
          return _listPropertyMapper.GetElement( obj, _index );
        }

        public void SetObjectValue( object obj, object value )
        {
          _listPropertyMapper.SetElement( obj, _index, value );
        }

        public PropertyVisibility GetVisibility( object obj )
        {
          return PropertyVisibility.Editable;
        }

        public bool IsEditableForBlockPatternChildren => _listPropertyMapper.IsEditableForBlockPatternChildren;

        public IEnumerable GetValueList( object obj )
        {
          return _listPropertyMapper.GetValueList( obj );
        }
      }

      #endregion

      #region Reflection

      private static Func<object, int> CreateCountGetter( Type listType, Func<object, object> propGetter )
      {
        var prop = GetProperty( listType, "Count", typeof( int ) );
        if ( null == prop ) throw new InvalidProgramException( "Property `Count` is not found in `IList<T>`." );

        return CreateFunction<Func<object, int>>( listType, propGetter, prop.GetGetMethod(), Type.EmptyTypes, typeof( int ) );
      }
      private static Action<object, int> CreateCountSetter( Type listType, Func<object, object> propGetter )
      {
        var prop = GetProperty( listType, "Count", typeof( int ) );
        if ( null == prop ) throw new InvalidProgramException( "Property `Count` is not found in `IList<T>`." );

        return CreateFunction<Action<object, int>>( listType, propGetter, prop.GetSetMethod(), new Type[1] { typeof( int ) }, typeof( void ) );
      }

      private static Func<object, bool> CreateReadOnlyGetter( Type listType, Func<object, object> propGetter )
      {
        var prop = GetProperty( listType, "IsReadOnly", typeof( bool ) );
        if ( null == prop ) throw new InvalidProgramException( "Property `Count` is not found in `IList<T>`." );

        return CreateFunction<Func<object, bool>>( listType, propGetter, prop.GetGetMethod(), Type.EmptyTypes, typeof( bool ) );
      }

      private static Func<object, int, object> CreateElementGetter( Type listType, Func<object, object> propGetter )
      {
        var indexer = GetIndexer( listType, typeof( int ) );
        if ( null == indexer ) throw new InvalidProgramException( "operator this[int] is not found in `IList<T>`." );

        return CreateFunction<Func<object, int, object>>( listType, propGetter, indexer.GetGetMethod(), new Type[1] { typeof( int ) }, typeof( object ) );
      }
      private static Action<object, int, object> CreateElementSetter( Type listType, Func<object, object> propGetter )
      {
        var indexer = GetIndexer( listType, typeof( int ) );
        if ( null == indexer ) throw new InvalidProgramException( "operator this[int] is not found in `IList<T>`." );

        return CreateFunction<Action<object, int, object>>( listType, propGetter, indexer.GetSetMethod(), new Type[2] { typeof( int ), typeof( object ) }, typeof( void ) );
      }

      #endregion
    }

    private class ObjectPropertyMapper : IPropertyMapper
    {
      public long UniqueToken { get; private set; }
      public PropertyCategory Category { get; private set; }
      public ValueType ValueType { get { return ValueType.Composite; } }
      public string Name { get; private set; }
      public string Label { get; private set; }
      public int Order { get; private set; }
      public bool IsEditableForBlockPatternChildren { get; }

      private readonly Func<object, object> _getter;
      private readonly Func<object, bool> _isEditableGetter;
      private readonly Func<object, bool> _isVisibleGetter;
      private readonly Action<object> _postSetter;

      public ObjectPropertyMapper(
        Type objectType,
        PropertyCategory category, string name, int order, string label, bool isEditableForBlockPatternChildren,
      long uniqueToken,
        Func<object, object> getter,
        Func<object, bool> isEditableGetter,
        Func<object, bool> isVisibleGetter,
        Action<object> postSetter )
      {
        UniqueToken = uniqueToken;
        IsEditableForBlockPatternChildren = isEditableForBlockPatternChildren;

        Category = category;
        Name = name;
        Order = order;
        Label = label ?? name;

        _getter = getter;
        _isEditableGetter = isEditableGetter;
        _isVisibleGetter = isVisibleGetter;
        _postSetter = postSetter;
      }
      
      public INamedProperty GetPropertyInfo( object obj )
      {
        return null ;
      }

      public object GetObjectValue( object obj )
      {
        return _getter?.Invoke( obj );
      }

      public void SetObjectValue( object obj, object value )
      {
        // GetObjectValue()経由で扱うため、_postSetterのみ設定
        _postSetter?.Invoke( obj );
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        if ( null == _getter ) return PropertyVisibility.Hidden;

        if ( null != _isVisibleGetter && false == _isVisibleGetter( obj ) ) {
          // 強制非表示
          return PropertyVisibility.Hidden;
        }
        if ( null != _isEditableGetter && false == _isEditableGetter( obj ) ) {
          // ReadOnlyオブジェクトは編集時もReadOnly
          return PropertyVisibility.ReadOnly;
        }

        return PropertyVisibility.Editable;
      }

      public IEnumerable GetValueList( object obj )
      {
        return null ;
      }
    }

    private class NamedPropertyObjectValueMapper : IObjectValueMapper
    {
      private readonly bool _isInvisible;
      private readonly string _propertyName;

      public NamedPropertyObjectValueMapper( string propertyName, PropertyType type )
      {
        _propertyName = propertyName;
        ValueType = ToValueType( type );
        _isInvisible = (PropertyType.TemporaryValue == type);
      }

      public string Label => _propertyName ;

      public ValueType ValueType { get; }

      public bool IsEditableForBlockPatternChildren => false;
      
      public INamedProperty GetPropertyInfo( object obj )
      {
        var entity = obj as Entity;
        return entity?.GetProperty( _propertyName );
      }

      public object GetObjectValue( object obj )
      {
        var entity = obj as Entity;
        if ( null == entity ) return 0.0;
        return entity.GetProperty( _propertyName ).Value;
      }

      public void SetObjectValue( object obj, object value )
      {
        var entity = obj as Entity;
        if ( null == entity ) return;
        if ( value is int ) {
          entity.GetProperty( _propertyName ).Value = (int)value;
        }
        else {
          entity.GetProperty( _propertyName ).Value = (double)value;
        }
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        if ( _isInvisible ) return PropertyVisibility.Hidden;
        return (obj is Entity) ? PropertyVisibility.Editable : PropertyVisibility.Hidden;
      }

      public IEnumerable GetValueList( object obj )
      {
        return null ;
      }
    }

    private class EnumNamedPropertyObjectValueMapper : IObjectValueMapper
    {
      private readonly string _propertyName;

      public EnumNamedPropertyObjectValueMapper( string propertyName )
      {
        _propertyName = propertyName;
      }

      public string Label => _propertyName ;

      public ValueType ValueType => ValueType.Select ;

      public bool IsEditableForBlockPatternChildren => false ;
      
      public INamedProperty GetPropertyInfo( object obj )
      {
        var entity = obj as Entity;
        return entity?.GetProperty( _propertyName );
      }

      public object GetObjectValue( object obj )
      {
        if ( obj is Entity entity ) {
          return entity.GetProperty( _propertyName ).Value ;
        }

        return 0.0 ;
      }

      public void SetObjectValue( object obj, object value )
      {
        if ( obj is Entity entity ) {
          entity.GetProperty( _propertyName ).Value = (double) value ;
        }
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        return (obj is Entity) ? PropertyVisibility.Editable : PropertyVisibility.Hidden;
      }

      public IEnumerable GetValueList( object obj )
      {
        return null ;
      }
    }

    private class SteppedNamedPropertyObjectValueMapper : IObjectValueMapper
    {
      private readonly bool _isInvisible;
      private readonly string _propertyName;

      public SteppedNamedPropertyObjectValueMapper( string propertyName, PropertyType type )
      {
        _propertyName = propertyName;
        ValueType = ToValueType( type );
        _isInvisible = (PropertyType.TemporaryValue == type);
      }

      public string Label => _propertyName ;

      public ValueType ValueType { get; }

      public bool IsEditableForBlockPatternChildren => false;
      
      public INamedProperty GetPropertyInfo( object obj )
      {
        var entity = obj as Entity;
        return entity?.GetProperty( _propertyName );
      }

      public object GetObjectValue( object obj )
      {
        if ( obj is Entity entity ) {
          return entity.GetProperty( _propertyName ).Value ;
        }

        return 0.0 ;
      }

      public void SetObjectValue( object obj, object value )
      {
        if ( obj is Entity entity ) {
          entity.GetProperty( _propertyName ).Value = (double) value ;
        }
      }

      public PropertyVisibility GetVisibility( object obj )
      {
        return (obj is Entity) ? PropertyVisibility.Editable : PropertyVisibility.Hidden;
      }
    
      public IEnumerable GetValueList( object obj )
      {
        return null ;
      }
    }
  }
}
