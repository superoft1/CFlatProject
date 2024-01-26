using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chiyoda
{
  class PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>
    where TCommonType : class
    where TPropertyAttribute : Attribute
  {
    public Type DeclaringType { get; private set; }

    public string PropertyName { get; private set; }

    public MethodInfo GetterMethod { get; private set; }
    public TPropertyAttribute GetterAttribute { get; private set; }

    public MethodInfo SetterMethod { get; private set; }
    public TPropertyAttribute SetterAttribute { get; private set; }

    public long UniqueToken { get; private set; }

    private Func<TCommonType, object> _getter = null;
    public Func<TCommonType, object> Getter
    {
      get
      {
        if ( null == _getter ) {
          _getter = (null == GetterMethod ? null : GetGetterLambda( DeclaringType, GetterMethod ));
        }
        return _getter;
      }
    }

    private Action<TCommonType, object> _setter = null;
    public Action<TCommonType, object> Setter
    {
      get
      {
        if ( null == _setter ) {
          _setter = (null == SetterMethod ? null : GetSetterLambda( DeclaringType, SetterMethod ));
        }
        return _setter;
      }
    }

    public PropertyGetterSetterInfo( Type declaringType, string name, MethodInfo getter, TPropertyAttribute getterProperty, MethodInfo setter, TPropertyAttribute setterProperty )
    {
      DeclaringType = declaringType;
      PropertyName = name;
      GetterMethod = getter;
      GetterAttribute = getterProperty;
      SetterMethod = setter;
      SetterAttribute = setterProperty;

      unchecked {
        ulong getterToken = (null == GetterMethod) ? 0ul : (ulong)(uint)GetterMethod.GetBaseDefinition().MetadataToken;
        ulong setterToken = (null == SetterMethod) ? 0ul : (ulong)(uint)SetterMethod.GetBaseDefinition().MetadataToken;

        UniqueToken = (long)(getterToken << 32 | setterToken);
      }
    }

    private static Func<TCommonType, object> GetGetterLambda( Type type, MethodInfo getMethod )
    {
      var thisParam = Expression.Parameter( typeof( TCommonType ), "entity" );

      var parameters = getMethod.GetParameters();
      if ( false == getMethod.IsStatic && 0 == parameters.Length ) {
        return Expression.Lambda<Func<TCommonType, object>>(
            Expression.Convert(
              Expression.Call( Expression.TypeAs( thisParam, getMethod.DeclaringType ), getMethod ),
              typeof( object )
            ), thisParam
          ).Compile();
      }
      if ( true == getMethod.IsStatic && 1 == parameters.Length && parameters[0].ParameterType.IsAssignableFrom( type ) ) {
        return Expression.Lambda<Func<TCommonType, object>>(
            Expression.Convert(
              Expression.Call( getMethod, Expression.TypeAs( thisParam, parameters[0].ParameterType ) ),
              typeof( object )
            ), thisParam
          ).Compile();
      }

      throw new InvalidProgramException( "Property getter method must be nonstatic method with no params or static method with only one param." );
    }

    private static Action<TCommonType, object> GetSetterLambda( Type type, MethodInfo setMethod )
    {
      var thisParam = Expression.Parameter( typeof( TCommonType ), "entity" );
      var arg = Expression.Parameter( typeof( object ), "value" );

      var parameters = setMethod.GetParameters();
      if ( false == setMethod.IsStatic && 1 == parameters.Length ) {
        return Expression.Lambda<Action<TCommonType, object>>(
          Expression.Call(
            Expression.TypeAs( thisParam, setMethod.DeclaringType ),
            setMethod,
            Expression.Convert( arg, parameters[0].ParameterType )
          ), thisParam, arg
        ).Compile();
      }
      if ( true == setMethod.IsStatic && 2 == parameters.Length && parameters[0].ParameterType.IsAssignableFrom( type ) ) {
        return Expression.Lambda<Action<TCommonType, object>>(
          Expression.Call(
            setMethod,
            Expression.TypeAs( thisParam, parameters[0].ParameterType ),
            Expression.Convert( arg, parameters[1].ParameterType )
          ), thisParam, arg
        ).Compile();
      }
      if ( false == setMethod.IsStatic && 0 == parameters.Length ) {
        return Expression.Lambda<Action<TCommonType, object>>(
          Expression.Call(
            Expression.TypeAs( thisParam, setMethod.DeclaringType ),
            setMethod
          ), thisParam, arg
        ).Compile();
      }
      if ( true == setMethod.IsStatic && 1 == parameters.Length && parameters[0].ParameterType.IsAssignableFrom( type ) ) {
        return Expression.Lambda<Action<TCommonType, object>>(
          Expression.Call(
            setMethod,
            Expression.TypeAs( thisParam, parameters[0].ParameterType )
          ), thisParam, arg
        ).Compile();
      }

      throw new InvalidProgramException( "Property setter method must be nonstatic method with one param or static method with only two params." );
    }

  }

  static class AttributeExtension
  {
    public static IEnumerable<PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>> CollectProperties<TCommonType, TPropertyAttribute>( this Type type, bool allowGetterOnly )
      where TCommonType : class
      where TPropertyAttribute : Attribute
    {
      foreach ( var prop in type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        var attr = prop.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        if ( null == prop.GetGetMethod( true ) ) continue;

        yield return new PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>( type, prop.Name, prop.GetGetMethod( true ), attr, prop.GetSetMethod( true ), attr );
      }
    }

    public static IEnumerable<PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>> CollectActions<TCommonType, TPropertyAttribute>( this Type type, bool allowGetterOnly )
      where TCommonType : class
      where TPropertyAttribute : Attribute
    {
      foreach ( var method in type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        if ( 0 != method.GetParameters().Length || typeof( void ) != method.ReturnType ) continue;

        var name = GetPropertyNameFromMethodName( method.Name );
        yield return new PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>( type, name, null, attr, method, attr );
      }
    }

    public static IEnumerable<PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>> CollectActions<TCommonType, TPropertyAttribute>( this Type type, IEnumerable<MethodInfo> extMethods, bool allowGetterOnly )
      where TCommonType : class
      where TPropertyAttribute : Attribute
    {
      foreach ( var method in extMethods ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        var parameters = method.GetParameters();
        if ( 1 != parameters.Length || typeof( void ) != method.ReturnType ) continue;
        if ( false == parameters[0].ParameterType.IsAssignableFrom( type ) ) continue;

        var name = GetPropertyNameFromMethodName( method.Name );
        yield return new PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>( type, name, null, attr, method, attr );
      }
    }

    public static IEnumerable<PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>> CollectGetterSetterPairs<TCommonType, TPropertyAttribute>( this Type type, IEqualityComparer<TPropertyAttribute> comparer, bool allowGetterOnly )
      where TCommonType : class
      where TPropertyAttribute : Attribute
    {
      var dicSetters = new Dictionary<TPropertyAttribute, KeyValuePair<MethodInfo, TPropertyAttribute>>( comparer );

      // check setters
      foreach ( var method in type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        if ( 1 != method.GetParameters().Length || typeof( void ) != method.ReturnType ) continue;

        if ( dicSetters.ContainsKey( attr ) ) continue;

        dicSetters.Add( attr, new KeyValuePair<MethodInfo, TPropertyAttribute>( method, attr ) );
      }

      // check getters
      foreach ( var method in type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        if ( 0 != method.GetParameters().Length ) continue;

        KeyValuePair<MethodInfo, TPropertyAttribute> setterPair;
        if ( dicSetters.TryGetValue( attr, out setterPair ) ) {
          if ( setterPair.Key.GetParameters()[0].ParameterType != method.ReturnType ) {
            setterPair = new KeyValuePair<MethodInfo, TPropertyAttribute>( null, null );
          }
        }
        else {
          setterPair = new KeyValuePair<MethodInfo, TPropertyAttribute>( null, null );
        }

        if ( !allowGetterOnly ) {
          if ( null == setterPair.Key ) {
            throw new InvalidProgramException( "Setter is not found." );
          }
        }

        var name = GetPropertyNameFromMethodName( method.Name );

        yield return new PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>( type, name, method, attr, setterPair.Key, setterPair.Value );
      }
    }

    public static IEnumerable<PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>> CollectGetterSetterPairs<TCommonType, TPropertyAttribute>( this Type type, IEnumerable<MethodInfo> extMethods, IEqualityComparer<TPropertyAttribute> comparer, bool allowGetterOnly )
      where TCommonType : class
      where TPropertyAttribute : Attribute
    {
      var dicSetters = new Dictionary<TPropertyAttribute, KeyValuePair<MethodInfo, TPropertyAttribute>>( comparer );

      // check setters
      foreach ( var method in extMethods ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        var parameters = method.GetParameters();
        if ( 2 != parameters.Length || typeof( void ) != method.ReturnType ) continue;
        if ( false == parameters[0].ParameterType.IsAssignableFrom( type ) ) continue;

        if ( dicSetters.ContainsKey( attr ) ) continue;

        dicSetters.Add( attr, new KeyValuePair<MethodInfo, TPropertyAttribute>( method, attr ) );
      }

      // check getters
      foreach ( var method in extMethods ) {
        var attr = method.GetCustomAttributes( typeof( TPropertyAttribute ), false ).FirstOrDefault() as TPropertyAttribute;
        if ( null == attr ) continue;

        var parameters = method.GetParameters();
        if ( 1 != parameters.Length ) continue;
        if ( false == parameters[0].ParameterType.IsAssignableFrom( type ) ) continue;

        KeyValuePair<MethodInfo, TPropertyAttribute> setterPair;
        if ( dicSetters.TryGetValue( attr, out setterPair ) ) {
          if ( setterPair.Key.GetParameters()[1].ParameterType != method.ReturnType ) {
            setterPair = new KeyValuePair<MethodInfo, TPropertyAttribute>( null, null );
          }
        }
        else {
          setterPair = new KeyValuePair<MethodInfo, TPropertyAttribute>( null, null );
        }

        if ( !allowGetterOnly ) {
          if ( null == setterPair.Key ) {
            throw new InvalidProgramException( "Setter is not found." );
          }
        }

        var name = GetPropertyNameFromMethodName( method.Name );

        yield return new PropertyGetterSetterInfo<TCommonType, TPropertyAttribute>( type, name, method, attr, setterPair.Key, setterPair.Value );
      }
    }

    private static string GetPropertyNameFromMethodName( string methodName )
    {
      if ( 3 < methodName.Length && methodName.StartsWith( "Get" ) && char.IsUpper( methodName[3] ) ) {
        return methodName.Substring( 3 );
      }

      return methodName;
    }
  }
}
