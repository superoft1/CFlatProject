using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Core
{
  [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
  class PropertyExpressionFunctionAttribute : Attribute
  {
    /// <summary>
    /// 可変長引数かどうか。
    /// </summary>
    public bool VariableLengthArgument { get; set; }

    /// <summary>
    /// 引数の数（可変長の際は最少引数数）
    /// </summary>
    public int ArgumentCount { get; set; }

    /// <summary>
    /// 関数名
    /// </summary>
    public string Name { get; private set; }

    public PropertyExpressionFunctionAttribute( string name )
    {
      Name = name;
    }

    public delegate double PropertyFunction( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args );

    public class FunctionInfo
    {
      public PropertyFunction Function { get; }
      public bool VariableLengthArgument { get; }
      public int ArgumentCount { get; }

      public FunctionInfo( PropertyFunction func, bool variableLength, int argCount )
      {
        Function = func;
        VariableLengthArgument = variableLength;
        ArgumentCount = argCount;
      }
    }

    private static readonly Dictionary<string, FunctionInfo> _funcs = new Dictionary<string, FunctionInfo>();

    public static bool ExistsFunction( string name )
    {
      return _funcs.ContainsKey( name.ToLower() );
    }

    public static FunctionInfo GetFunctionInfo( string name )
    {
      if ( !_funcs.TryGetValue( name.ToLower(), out var info ) ) throw new ArgumentOutOfRangeException( nameof( name ) );

      return info;
    }

    static PropertyExpressionFunctionAttribute()
    {
      RegisterFunctions( typeof( PropertyExpressionFunctions ) );
    }

    private static void RegisterFunctions( Type type )
    {
      foreach ( var method in type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        if ( method.ReturnType != typeof( double ) ) continue;
        var paramInfos = method.GetParameters();
        if ( 3 != paramInfos.Length ) continue;

        if ( typeof( PropertyExpressionContext ) != paramInfos[0].ParameterType ) continue;
        if ( typeof( IPropertiedElement ) != paramInfos[1].ParameterType ) continue;
        if ( typeof( IPropertyExpression[] ) != paramInfos[2].ParameterType ) continue;

        var attrs = method.GetCustomAttributes( typeof( PropertyExpressionFunctionAttribute ), false );
        if ( 0 == attrs.Length ) continue;

        var func = Delegate.CreateDelegate( typeof( PropertyFunction ), method ) as PropertyFunction;
        if ( null == func ) continue;

        foreach ( PropertyExpressionFunctionAttribute attr in attrs ) {
          var name = attr.Name.ToLower();
          if ( string.IsNullOrWhiteSpace( name ) ) continue ;

          if ( _funcs.ContainsKey( name ) ) continue;
          _funcs.Add( name, new FunctionInfo( func, attr.VariableLengthArgument, attr.ArgumentCount ) );
        }
      }
    }

    public static IEnumerable<string> GetAllNames()
    {
      return _funcs.Keys.OrderByDescending( x => x.Length ) ;
    }
  }


  
  interface IMetaFunction
  {
    double GetValue( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args ) ;

    IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm, IPropertyExpression[] args ) ;
  }

  [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
  class PropertyExpressionMetaFunctionAttribute : Attribute
  {
    /// <summary>
    /// 可変長引数かどうか。
    /// </summary>
    public bool VariableLengthArgument { get; set; }

    /// <summary>
    /// 引数の数（可変長の際は最少引数数）
    /// </summary>
    public int ArgumentCount { get; set; }

    /// <summary>
    /// 関数名
    /// </summary>
    public string Name { get; }

    public PropertyExpressionMetaFunctionAttribute( string name )
    {
      Name = name;
    }

    public class MetaFunctionInfo
    {
      public IMetaFunction MetaFunctionClass { get; }
      public bool VariableLengthArgument { get; }
      public int ArgumentCount { get; }

      public MetaFunctionInfo( IMetaFunction func, bool variableLength, int argCount )
      {
        MetaFunctionClass = func;
        VariableLengthArgument = variableLength;
        ArgumentCount = argCount;
      }
    }

    private static readonly Dictionary<string, MetaFunctionInfo> _funcs = new Dictionary<string, MetaFunctionInfo>();

    public static bool ExistsMetaFunction( string name )
    {
      return _funcs.ContainsKey( name.ToLower() );
    }

    public static MetaFunctionInfo GetMetaFunctionInfo( string name )
    {
      if ( !_funcs.TryGetValue( name.ToLower(), out var info ) ) throw new ArgumentOutOfRangeException( nameof( name ) );

      return info;
    }

    static PropertyExpressionMetaFunctionAttribute()
    {
      RegisterMetaFunctions( typeof( PropertyExpressionFunctions ) );
    }

    private static void RegisterMetaFunctions( Type type )
    {
      foreach ( var subClass in type.GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic ) ) {
        if ( !typeof( IMetaFunction ).IsAssignableFrom( subClass ) ) continue ;

        var attrs = subClass.GetCustomAttributes( typeof( PropertyExpressionMetaFunctionAttribute ), false );
        if ( 0 == attrs.Length ) continue;

        var instance = (IMetaFunction) Activator.CreateInstance( subClass ) ;

        foreach ( PropertyExpressionMetaFunctionAttribute attr in attrs ) {
          var name = attr.Name.ToLower();
          if ( string.IsNullOrWhiteSpace( name ) ) continue ;

          if ( _funcs.ContainsKey( name ) ) continue;
          _funcs.Add( name, new MetaFunctionInfo( instance, attr.VariableLengthArgument, attr.ArgumentCount ) );
        }
      }
    }

    public static IEnumerable<string> GetAllNames()
    {
      return _funcs.Keys.OrderByDescending( x => x.Length ) ;
    }
  }
  

  
  [AttributeUsage( AttributeTargets.Field, AllowMultiple = true )]
  class PropertyExpressionConstantAttribute : Attribute
  {
    /// <summary>
    /// 定数名
    /// </summary>
    public string Name { get; }

    public PropertyExpressionConstantAttribute( string name )
    {
      Name = name;
    }


    private static readonly Dictionary<string, double> _constants = new Dictionary<string, double>();

    public static bool ExistsConstant( string name )
    {
      return _constants.ContainsKey( name.ToLower() );
    }

    public static double GetValue( string name )
    {
      if ( !_constants.TryGetValue( name.ToLower(), out var value ) ) throw new ArgumentOutOfRangeException( nameof( name ) );

      return value;
    }

    static PropertyExpressionConstantAttribute()
    {
      RegisterConstants( typeof( PropertyExpressionFunctions ) );
    }

    private static void RegisterConstants( Type type )
    {
      foreach ( var field in type.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) ) {
        if ( field.FieldType != typeof( double ) ) continue;

        var attrs = field.GetCustomAttributes( typeof( PropertyExpressionConstantAttribute ), false );
        if ( 0 == attrs.Length ) continue;

        double value = (double)field.GetValue( null );
        foreach ( PropertyExpressionConstantAttribute attr in attrs ) {
          var name = attr.Name?.ToLower();
          if ( string.IsNullOrWhiteSpace( name ) ) continue ;

          if ( _constants.ContainsKey( name ) ) continue;
          _constants.Add( name, value );
        }
      }
    }

    public static IEnumerable<string> GetAllNames()
    {
      return _constants.Keys.OrderByDescending( x => x.Length ) ;
    }
  }
}
