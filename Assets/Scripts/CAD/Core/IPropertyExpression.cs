using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// プロパティ式表現。
  /// </summary>
  public interface IPropertyExpression
  {
    /// <summary>
    /// プロパティ値。
    /// </summary>
    double GetValue( PropertyExpressionContext context, IPropertiedElement elm );

    /// <summary>
    /// 関連する全プロパティ値。
    /// </summary>
    /// <param name="elm">適用対象要素。</param>
    /// <returns>関連オブジェクトとプロパティのペアの列挙。</returns>
    IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm );
  }

  /// <summary>
  /// 即値 プロパティ式表現。
  /// </summary>
  public class NumericExpression : IPropertyExpression
  {
    public enum NumericUnit
    {
      None,
      MilliMeters,
      CentiMeters,
      Meters,
      Inches,
      Feet,
      Yards,
      Percent,
    }

    #region 単位情報

    private class NumericUnitInfo
    {
      public string Name { get; private set; }
      public double Ratio { get; private set; }

      public NumericUnitInfo( string name, double ratio )
      {
        Name = name;
        Ratio = ratio;
      }
    }

    private static readonly Dictionary<int, NumericUnitInfo> _dic;  // Enum キーだと1ケタ遅いので、NumericUnit ではなく int をキーに
    private static readonly Dictionary<string, NumericUnit> _reverseDic;

    static NumericExpression()
    {
      _dic = new Dictionary<int, NumericUnitInfo>
      {
        { (int)NumericUnit.None, new NumericUnitInfo( "", 1 ) },
        { (int)NumericUnit.MilliMeters, new NumericUnitInfo( "mm", 0.001 ) },
        { (int)NumericUnit.CentiMeters, new NumericUnitInfo( "cm", 0.01 ) },
        { (int)NumericUnit.Meters, new NumericUnitInfo( "m", 1 ) },
        { (int)NumericUnit.Inches, new NumericUnitInfo( "in", 0.0254 ) },
        { (int)NumericUnit.Feet, new NumericUnitInfo( "ft", 0.3048 ) },
        { (int)NumericUnit.Yards, new NumericUnitInfo( "yd", 0.9144 ) },
        { (int)NumericUnit.Percent, new NumericUnitInfo( "%", 0.01 ) },
      };

      _reverseDic = new Dictionary<string, NumericUnit>();
      foreach ( var pair in _dic ) {
        _reverseDic.Add( pair.Value.Name, (NumericUnit)pair.Key );
      }
    }

    #endregion

    private readonly NumericUnitInfo _unitInfo;

    public static NumericUnit? GetUnit( string unitName )
    {
      if ( null == unitName ) return NumericUnit.None;
      NumericUnit unit;
      if ( _reverseDic.TryGetValue( unitName.ToLower(), out unit ) ) return unit;

      return null;
    }

    private static NumericUnitInfo GetUnitInfo( NumericUnit unit )
    {
      NumericUnitInfo info;
      if ( _dic.TryGetValue( (int)unit, out info ) ) return info;

      throw new InvalidProgramException();
    }

    public double RawValue { get; private set; }

    public NumericUnit Unit { get; private set; }

    public string UnitName { get { return _unitInfo.Name; } }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      return RawValue * _unitInfo.Ratio;
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      yield break;
    }

    public override string ToString()
    {
      return RawValue.ToString( "0.######" ) + _unitInfo.Name;
    }

    public NumericExpression( double rawValue, NumericUnit unit )
    {
      Unit = unit;
      _unitInfo = GetUnitInfo( unit );
      RawValue = rawValue;
    }

    public static IEnumerable<string> GetAllUnitNames()
    {
      foreach ( var value in _dic.Values ) {
        if ( "" != value.Name ) yield return value.Name;
      }
    }
  }

  /// <summary>
  /// 定数 プロパティ式表現。
  /// </summary>
  public class ConstantExpression : IPropertyExpression
  {
    public string ConstantName { get; }

    public double? Value { get; }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      if (Value == null && (context==null || context.GetVariable(ConstantName) == null)){
        UnityEngine.Debug.Log($"{ConstantName} not found");
      }
      return Value ?? context?.GetVariable( ConstantName ) ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.VariableNotFound, ConstantName );
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      yield break;
    }

    public override string ToString()
    {
      return ConstantName;
    }

    public ConstantExpression( string name )
    {
      if ( PropertyExpressionConstantAttribute.ExistsConstant( name ) ) {
        Value = PropertyExpressionConstantAttribute.GetValue( name ) ;
      }
      else {
        Value = null ;
      }

      ConstantName = name;
    }

    public static IEnumerable<string> GetAllConstantNames()
    {
      return PropertyExpressionConstantAttribute.GetAllNames();
    }
  }

  /// <summary>
  /// オブジェクト式表現。
  /// </summary>
  public abstract class ObjectExpression : IPropertyExpression
  {
    /// <summary>
    /// プロパティ値。
    /// </summary>
    double IPropertyExpression.GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement ) ;
    }

    public abstract IPropertiedElement GetObject( IPropertiedElement elm ) ;

    /// <summary>
    /// 関連する全プロパティ値。
    /// </summary>
    /// <param name="elm">適用対象要素。</param>
    /// <returns>関連オブジェクトとプロパティのペアの列挙。</returns>
    IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> IPropertyExpression.GetSourceProperties( IPropertiedElement elm )
    {
      return Array.Empty<KeyValuePair<IPropertiedElement, INamedProperty>>() ;
    }
  }

  /// <summary>
  /// オブジェクト プロパティ式表現。
  /// </summary>
  public class SimpleObjectExpression : ObjectExpression
  {
    public string ObjectName { get; private set; }

    public override IPropertiedElement GetObject( IPropertiedElement elm )
    {
      return elm.GetElementByName( ObjectName );
    }

    public override string ToString()
    {
      if ( 0 < ObjectName.Length && ':' == ObjectName[ 0 ] ) {
        return ObjectName ;
      }
      else {
        return "#" + ObjectName ;
      }
    }

    public SimpleObjectExpression( string objectName )
    {
      if ( null == objectName ) throw new ArgumentNullException( nameof( objectName ) );

      ObjectName = objectName;
    }
  }

  /// <summary>
  /// オブジェクト プロパティ式表現。
  /// </summary>
  public class ObjectPropertyExpression : IPropertyExpression
  {
    public string ObjectName { get; private set; }
    public string PropertyName { get; private set; }

    private KeyValuePair<IPropertiedElement, INamedProperty> GetSource( IPropertiedElement elm )
    {
      if ( null != ObjectName ) {
        elm = elm.GetElementByName( ObjectName );
        if ( null == elm ) {
          return new KeyValuePair<IPropertiedElement, INamedProperty>( null, null );
        }
      }
      return new KeyValuePair<IPropertiedElement, INamedProperty>( elm, elm.GetProperty( PropertyName ) );
    }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      var source = GetSource( elm );
      if ( null == source.Key ) {
        UnityEngine.Debug.Log($"{ObjectName} not found");
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, ObjectName );
      }
      if ( null == source.Value ) {
        UnityEngine.Debug.Log($"{ObjectName}.{PropertyName} not found");
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.PropertyNotFound, ObjectName, PropertyName );
      }

      return source.Value.Value;
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      var source = GetSource( elm );
      if ( null == source.Value ) yield break;

      yield return source;
    }

    public override string ToString()
    {
      if ( null == ObjectName ) {
        return "." + PropertyName ;
      }
      else if ( 0 < ObjectName.Length && ':' == ObjectName[ 0 ] ) {
        return ObjectName + "." + PropertyName ;
      }
      else {
        return "#" + ObjectName + "." + PropertyName ;
      }
    }

    public ObjectPropertyExpression( string objectName, string propertyName )
    {
      if ( null == propertyName ) throw new ArgumentNullException( nameof( propertyName ) );

      ObjectName = objectName;
      PropertyName = propertyName;
    }
  }

  /// <summary>
  /// 単項演算 プロパティ式表現。
  /// </summary>
  public class UnaryOperatorExpression : IPropertyExpression
  {
    public enum OperatorType
    {
      Negate,
    }

    #region 単位情報

    private class OperatorInfo
    {
      public string Name { get; private set; }

      private readonly Func<double, double> _operation;

      public double Invoke( double value ) { return _operation( value ); }

      public OperatorInfo( string name, Func<double, double> operation )
      {
        Name = name;
        _operation = operation;
      }
    }

    private static readonly Dictionary<int, OperatorInfo> _dic;  // Enum キーだと1ケタ遅いので、OperatorType ではなく int をキーに
    private static readonly Dictionary<string, OperatorType> _reverseDic;

    static UnaryOperatorExpression()
    {
      _dic = new Dictionary<int, OperatorInfo>
      {
        { (int)OperatorType.Negate, new OperatorInfo( "-", value => -value ) },
      };

      _reverseDic = new Dictionary<string, OperatorType>();
      foreach ( var pair in _dic ) {
        _reverseDic.Add( pair.Value.Name, (OperatorType)pair.Key );
      }
    }

    #endregion

    public static OperatorType? GetOperator( string opName )
    {
      if ( null == opName ) return null;

      if ( _reverseDic.TryGetValue( opName, out var opType ) ) return opType;

      return null;
    }

    private static OperatorInfo GetOperatorInfo( OperatorType op )
    {
      if ( _dic.TryGetValue( (int)op, out var info ) ) return info;

      throw new InvalidProgramException();
    }

    private readonly OperatorType _opType;
    private readonly OperatorInfo _info;
    private readonly IPropertyExpression _expression;

    public OperatorType Operator { get { return _opType; } }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      return _info.Invoke( _expression.GetValue( context, elm ) );
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      return _expression.GetSourceProperties( elm );
    }

    public override string ToString()
    {
      return _info.Name + _expression.ToString();
    }

    public UnaryOperatorExpression( OperatorType opType, IPropertyExpression expression )
    {
      _opType = opType;
      _info = GetOperatorInfo( opType );
      _expression = expression;
    }
  }

  /// <summary>
  /// 二項演算 プロパティ式表現。
  /// </summary>
  public class BinaryOperatorExpression : IPropertyExpression
  {
    public enum OperatorType
    {
      Add,
      Subtract,
      Multiply,
      Divide,
      IsGreater,
      IsLess,
      IsGreaterOrEqual,
      IsLessOrEqual,
      EqualsTo,
      UnequalsTo,
    }

    #region 単位情報

    private class OperatorInfo
    {
      public string Name { get; private set; }

      private readonly Func<double, double, double> _operation;

      public double Invoke( double value1, double value2 ) { return _operation( value1, value2 ); }

      public OperatorInfo( string name, Func<double, double, double> operation )
      {
        Name = name;
        _operation = operation;
      }
    }

    private static readonly Dictionary<int, OperatorInfo> _dic; // Enum キーだと1ケタ遅いので、OperatorType ではなく int をキーに
    private static readonly Dictionary<string, OperatorType> _reverseDic;

    static BinaryOperatorExpression()
    {
      _dic = new Dictionary<int, OperatorInfo>
      {
        { (int)OperatorType.Add, new OperatorInfo( "+", ( x, y ) => x + y ) },
        { (int)OperatorType.Subtract, new OperatorInfo( "-", ( x, y ) => x - y ) },
        { (int)OperatorType.Multiply, new OperatorInfo( "*", ( x, y ) => x * y ) },
        { (int)OperatorType.Divide, new OperatorInfo( "/", ( x, y ) =>
          {
            if ( 0 == y ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.DividedByZero );
            return x / y;
          } ) },
        { (int)OperatorType.IsGreater, new OperatorInfo( ">", ( x, y ) => (x > y) ? 1 : 0 ) },
        { (int)OperatorType.IsLess, new OperatorInfo( "<", ( x, y ) => (x < y) ? 1 : 0 ) },
        { (int)OperatorType.IsGreaterOrEqual, new OperatorInfo( ">=", ( x, y ) => (x >= y) ? 1 : 0 ) },
        { (int)OperatorType.IsLessOrEqual, new OperatorInfo( "<=", ( x, y ) => (x <= y) ? 1 : 0 ) },
        { (int)OperatorType.EqualsTo, new OperatorInfo( "==", ( x, y ) => (x == y) ? 1 : 0 ) },
        { (int)OperatorType.UnequalsTo, new OperatorInfo( "!=", ( x, y ) => (x != y) ? 1 : 0 ) },
      };

      // エイリアス
      _reverseDic = new Dictionary<string, OperatorType>
      {
        { "=>", OperatorType.IsGreaterOrEqual },
        { "=<", OperatorType.IsLessOrEqual },
        { "=", OperatorType.EqualsTo },
        { "<>", OperatorType.UnequalsTo },
      };
      foreach ( var pair in _dic ) {
        _reverseDic.Add( pair.Value.Name, (OperatorType)pair.Key );
      }
    }

    #endregion

    public static OperatorType? GetOperator( string opName )
    {
      if ( null == opName ) return null;

      if ( _reverseDic.TryGetValue( opName, out var opType ) ) return opType;

      return null;
    }

    private static OperatorInfo GetOperatorInfo( OperatorType op )
    {
      if ( _dic.TryGetValue( (int)op, out var info ) ) return info;

      throw new InvalidProgramException();
    }

    private readonly OperatorType _opType;
    private readonly OperatorInfo _info;
    private readonly IPropertyExpression _expression1;
    private readonly IPropertyExpression _expression2;

    public OperatorType Operator { get { return _opType; } }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      double d1 = _expression1.GetValue( context, elm );
      double d2 = _expression2.GetValue( context, elm );

      return _info.Invoke( d1, d2 );
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      foreach ( var pair in _expression1.GetSourceProperties( elm ) ) yield return pair;
      foreach ( var pair in _expression2.GetSourceProperties( elm ) ) yield return pair;
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append( "(" );
      builder.Append( _expression1.ToString() );
      builder.Append( " " );
      builder.Append( _info.Name );
      builder.Append( " " );
      builder.Append( _expression2.ToString() );
      builder.Append( ")" );
      return builder.ToString();
    }

    public BinaryOperatorExpression( OperatorType opType, IPropertyExpression expression1, IPropertyExpression expression2 )
    {
      _opType = opType;
      _info = GetOperatorInfo( opType );
      _expression1 = expression1;
      _expression2 = expression2;
    }
  }

  /// <summary>
  /// 一時変数定義 プロパティ式表現
  /// </summary>
  public class VariableExpression : IPropertyExpression
  {
    public string VariableName { get ; }
    
    private readonly IPropertyExpression _definition;
    private readonly IPropertyExpression _expression;

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      if ( null == context ) context = new PropertyExpressionContext() ;

      var d1 = _definition.GetValue( context, elm );
      using ( context.SetVariable( VariableName, d1 ) ) {
        return _expression.GetValue( context, elm );
      }
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      foreach ( var pair in _definition.GetSourceProperties( elm ) ) yield return pair;
      foreach ( var pair in _expression.GetSourceProperties( elm ) ) yield return pair;
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append( "(" );
      builder.Append( VariableName );
      builder.Append( " := " );
      builder.Append( _definition.ToString() );
      builder.Append( ", " );
      builder.Append( _expression.ToString() );
      builder.Append( ")" );
      return builder.ToString();
    }
    
    public VariableExpression( string name, IPropertyExpression definition, IPropertyExpression expression )
    {
      VariableName = name ;
      _definition = definition ;
      _expression = expression ;
    }
  }

  /// <summary>
  /// 関数 プロパティ式表現。
  /// </summary>
  public class FunctionOperatorExpression : IPropertyExpression
  {
    private readonly PropertyExpressionFunctionAttribute.PropertyFunction _func;
    private readonly IPropertyExpression[] _expressions;

    public string FuncName { get; private set; }

    public IPropertyExpression[] Expressions
    {
      get { return _expressions.Clone() as IPropertyExpression[]; }
    }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      return _func( context, elm, _expressions );
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      foreach ( var expression in _expressions ) {
        foreach ( var pair in expression.GetSourceProperties( elm ) ) {
          yield return pair;
        }
      }
    }

    internal static IEnumerable<string> GetAllFunctionNames()
    {
      return PropertyExpressionFunctionAttribute.GetAllNames();
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append( FuncName );
      builder.Append( "(" );
      for ( int i = 0, n = _expressions.Length ; i < n ; ++i ) {
        if ( i != 0 ) builder.Append( ", " );
        builder.Append( _expressions[i].ToString() );
      }
      builder.Append( ")" );
      return builder.ToString();
    }

    public FunctionOperatorExpression( string name, params IPropertyExpression[] expressions )
    {
      FuncName = name;
      _func = PropertyExpressionFunctionAttribute.GetFunctionInfo( name ).Function;
      _expressions = expressions;
    }
  }

  /// <summary>
  /// メタ関数(引数にオブジェクト名を取ることのできる関数) プロパティ式表現。
  /// </summary>
  public class MetaFunctionOperatorExpression : IPropertyExpression
  {
    private readonly IMetaFunction _func;
    private readonly IPropertyExpression[] _expressions;

    public string FuncName { get; }

    public IPropertyExpression[] Expressions
    {
      get { return _expressions.Clone() as IPropertyExpression[]; }
    }

    public double GetValue( PropertyExpressionContext context, IPropertiedElement elm )
    {
      return _func.GetValue( context, elm, _expressions );
    }

    public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm )
    {
      return _func.GetSourceProperties( elm, _expressions ) ;
    }

    internal static IEnumerable<string> GetAllMetaFunctionNames()
    {
      return PropertyExpressionMetaFunctionAttribute.GetAllNames();
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.Append( FuncName );
      builder.Append( "(" );
      for ( int i = 0, n = _expressions.Length ; i < n ; ++i ) {
        if ( i != 0 ) builder.Append( ", " );
        builder.Append( _expressions[i].ToString() );
      }
      builder.Append( ")" );
      return builder.ToString();
    }

    public MetaFunctionOperatorExpression( string name, IPropertyExpression[] expressions )
    {
      var funcInfo = PropertyExpressionMetaFunctionAttribute.GetMetaFunctionInfo( name ) ;
      
      FuncName = name;
      _func = funcInfo.MetaFunctionClass;
      _expressions = expressions;
    }
  }
}
