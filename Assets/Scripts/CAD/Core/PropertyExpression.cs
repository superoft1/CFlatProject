using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Reflection ;
using System.Text ;
using System.Text.RegularExpressions ;
using Chiyoda.SyntaxTree ;

namespace Chiyoda.CAD.Core
{
  public static class PropertyExpression
  {
    /// <summary>
    /// プロパティ式を文字列からプロパティ表現クラスに変換します。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static IPropertyExpression ParseExpression( this string text )
    {
      if ( string.IsNullOrEmpty( text ) ) return null;

      var expression = PropertyExpressionDefinition.GetValueContextExpression();
      try {
        var tree = expression.Parse( text );
        return SyntaxTreeToPropertyExpressionConverter.Convert( tree );
      }
      catch ( SyntaxException ex ) {
        throw new PropertyParseException( text, ex );
      }
    }

    /// <summary>
    /// プロパティ指定を文字列からプロパティ表現クラスに変換します。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static ObjectPropertyExpression ParseObjectPropertyExpression( this string text )
    {
      var expression = PropertyExpressionDefinition.GetPropertyExpression();
      try {
        var tree = expression.Parse( text );
        var ope = SyntaxTreeToPropertyExpressionConverter.Convert( tree ) as ObjectPropertyExpression;
        if ( null == ope ) {
          throw new PropertyParseException( text, 0, PropertyParseErrorType.InvalidConstantName );
        }

        return ope;
      }
      catch ( SyntaxException ex ) {
        throw new PropertyParseException( text, ex );
      }
    }

    public static IElement GetSpecialElement( this IElement elm, string entityName )
    {
      if ( ':' != entityName[0] ) return null;

      if ( 0 == string.Compare( ":Root", entityName, true ) ) {
        return elm.Document;
      }
      if ( 0 == string.Compare( ":Parent", entityName, true ) ) {
        return elm.Parent;
      }
      if ( 0 == string.Compare( ":Self", entityName, true ) ) {
        return elm;
      }

      return null;
    }

    private static IEnumerable<string> GetSpecialObjectNames()
    {
      return new[] {
        "Root",
        "Parent",
        "Self",
      };
    }




    [AttributeUsage( AttributeTargets.Field )]
    private class ExpressionHierarchyAttribute : Attribute { }

    private static class PropertyExpressionDefinition
    {
      private const string CANNOT_USE_FOR_NAME = @"\s()/*+!#:.,<>=-" ;
      private const string CANNOT_USE_FOR_NAME_FIRST = "0-9" + CANNOT_USE_FOR_NAME ;

      private static readonly Regex NamePattern = new Regex( $"[^{CANNOT_USE_FOR_NAME_FIRST}][^{CANNOT_USE_FOR_NAME}]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant ) ;

      private static readonly SyntaxExpression Spaces = SyntaxExpression.FromRegExp( @"\s*" );

      [ExpressionHierarchy]
      private static readonly SyntaxExpression Numeric = SyntaxExpression.FromRegExp( @"[0-9]+(?:\.[0-9]*)?(?:e[+-]?[0-9]+)?", RegexOptions.IgnoreCase );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Unit = SyntaxExpression.NameOf( NamePattern, NumericExpression.GetAllUnitNames(), "単位名が正しくありません: \"{0}\"" ) ;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression NumericWithUnit = Numeric + Spaces + Unit;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression FunctionName = SyntaxExpression.NameOf( NamePattern, FunctionOperatorExpression.GetAllFunctionNames(), "関数名が正しくありません: \"{0}\"" ) ;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression MetaFunctionName = SyntaxExpression.NameOf( NamePattern, MetaFunctionOperatorExpression.GetAllMetaFunctionNames(), "関数名が正しくありません: \"{0}\"" ) ;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Constant = SyntaxExpression.FromRegExp( NamePattern ) ;
      private static readonly SyntaxExpression Value = Constant | NumericWithUnit | Numeric | SyntaxExpression.Empty( "値がありません" ) ;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Name = SyntaxExpression.FromRegExp( $"[^{CANNOT_USE_FOR_NAME}]+" );
      private static readonly SyntaxExpression NormalObjectName = "#" + ( Name | SyntaxExpression.Empty( "オブジェクト名が指定されていません" ) );
      private static readonly SyntaxExpression SpecialObjectName = ":" + SyntaxExpression.NameOf( NamePattern, GetSpecialObjectNames(), "特殊オブジェクト名が正しくありません: \"{0}\"" ) ;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression ObjectName = SpecialObjectName | NormalObjectName;
      private static readonly SyntaxExpression PropertyName = "." + ( Name | SyntaxExpression.Empty( "プロパティ名が指定されていません" ) );
      private static readonly SyntaxExpression ObjectPropertyName = ObjectName + Spaces + PropertyName;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Property = ObjectPropertyName | PropertyName;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression FunctionCall = FunctionName + Spaces + "(" + Spaces + SyntaxExpression.Repeat( SyntaxExpression.Placeholder( "ValueContext" ) + Spaces, 0, "," + Spaces ) + ( ")" | SyntaxExpression.Empty( "\")\" がありません" ) );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression MetaFunctionCall = MetaFunctionName + Spaces + "(" + Spaces + SyntaxExpression.Repeat( ObjectName + Spaces, 0, "," + Spaces ) + ( ")" | SyntaxExpression.Empty( "\")\" がありません" ) ) ;
      private static readonly SyntaxExpression Paren = "(" + Spaces + SyntaxExpression.Placeholder( "ValueContext" ) + Spaces + ( ")" | SyntaxExpression.Empty( "\")\" がありません" ) );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression VariableName = SyntaxExpression.FromRegExp( @"[a-z_][a-z_0-9]*", RegexOptions.IgnoreCase );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Variable = VariableName + Spaces + ":=" + Spaces + SyntaxExpression.Placeholder( "ValueContext" ) + Spaces + ( ( "," + Spaces + SyntaxExpression.Placeholder( "ValueContext" ) ) | SyntaxExpression.Empty( "変数を適用する式がありません" ) ) ;
      private static readonly SyntaxExpression Item = FunctionCall | MetaFunctionCall | Paren | Property | Value;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression Negate = "-" + Spaces + Item;
      [ExpressionHierarchy]
      private static readonly SyntaxExpression MultDivOperator = SyntaxExpression.FromRegExp( @"/|\*" );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression MultDiv = SyntaxExpression.Repeat( Negate | Item, 1, Spaces + MultDivOperator + Spaces );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression AddSubOperator = SyntaxExpression.FromRegExp( @"-|\+" );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression AddSub = SyntaxExpression.Repeat( MultDiv, 1, Spaces + AddSubOperator + Spaces );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression ComparisonOperator = SyntaxExpression.FromRegExp( "<>|!=|==|<=|>=|=>|=<|<|>|=" );
      [ExpressionHierarchy]
      private static readonly SyntaxExpression ValueContext = Spaces + ( Variable | SyntaxExpression.Repeat( AddSub, 1, 2, Spaces + ComparisonOperator + Spaces ) ) + Spaces ;

      [ExpressionHierarchy]
      private static readonly SyntaxExpression PropertyContext = Spaces + Property + Spaces;

      static PropertyExpressionDefinition()
      {
        FunctionCall.ReplacePlaceholder( "ValueContext", ValueContext );
        Paren.ReplacePlaceholder( "ValueContext", ValueContext );
        Variable.ReplacePlaceholder( "ValueContext", ValueContext );

        // Nameの登録
        foreach ( var field in typeof( PropertyExpressionDefinition ).GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) ) {
          if ( typeof( SyntaxExpression ) != field.FieldType ) continue;

          var xh = field.GetCustomAttributes( typeof( ExpressionHierarchyAttribute ), false );
          if ( null == xh || 0 == xh.Length ) continue; // 命名不要

          var expression = field.GetValue( null ) as SyntaxExpression;
          expression.Name = field.Name;
        }
      }

      private static string Join( string separator, IEnumerable<string> enumerable )
      {
        var builder = new StringBuilder();
        bool first = true;
        foreach ( var item in enumerable ) {
          if ( first ) {
            first = false;
          }
          else {
            builder.Append( separator );
          }
          builder.Append( item );
        }
        return builder.ToString();
      }

      public static SyntaxExpression GetValueContextExpression()
      {
        return ValueContext;
      }

      public static SyntaxExpression GetPropertyExpression()
      {
        return PropertyContext;
      }
    }

    private class SyntaxTreeToPropertyExpressionConverter
    {
      private delegate IPropertyExpression Converter( SyntaxTreeNode node );

      private static readonly Dictionary<string, Converter> _converters = new Dictionary<string, Converter>();

      static SyntaxTreeToPropertyExpressionConverter()
      {
        var myType = typeof( SyntaxTreeToPropertyExpressionConverter );

        // 変換メソッドの登録
        foreach ( var field in typeof( PropertyExpressionDefinition ).GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) ) {
          if ( typeof( SyntaxExpression ) != field.FieldType ) continue;

          var expression = field.GetValue( null ) as SyntaxExpression;
          if ( null == expression.Name ) continue;  // 名前がないものは登録しない

          var convertMethod = myType.GetMethod(
            "Convert" + expression.Name,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof( SyntaxTreeNode ) },
            new[] { new ParameterModifier( 1 ) } );
          if ( null == convertMethod || typeof( IPropertyExpression ) != convertMethod.ReturnType ) {
            continue;
          }

          var func = Delegate.CreateDelegate( typeof( Converter ), convertMethod ) as Converter;
          if ( null == func ) {
            continue;
          }

          _converters.Add( expression.Name, func );
        }
      }

      public static IPropertyExpression Convert( SyntaxTreeNode node )
      {
        Converter converter;
        if ( !_converters.TryGetValue( node.ExpressionName, out converter ) ) {
          throw new InvalidOperationException();
        }

        return converter( node );
      }

      private static IPropertyExpression ConvertPropertyContext( SyntaxTreeNode node )
      {
        // PropertyContext = Spaces + Property + Spaces;
        return ConvertLtoRBinaryOperator( node );
      }
      private static IPropertyExpression ConvertValueContext( SyntaxTreeNode node )
      {
        // ValueContext = Spaces + SyntaxExpression.Repeat( AddSub, 1, 2, Spaces + ComparisonOperator + Spaces ) + Spaces;
        return ConvertLtoRBinaryOperator( node );
      }
      private static IPropertyExpression ConvertAddSub( SyntaxTreeNode node )
      {
        // AddSub = SyntaxExpression.Repeat( MultDiv, 1, Spaces + AddSubOperator + Spaces );
        return ConvertLtoRBinaryOperator( node );
      }
      private static IPropertyExpression ConvertMultDiv( SyntaxTreeNode node )
      {
        // MultDiv = SyntaxExpression.Repeat( Negate | Item, 1, Spaces + MultDivOperator + Spaces );
        return ConvertLtoRBinaryOperator( node );
      }
      private static IPropertyExpression ConvertNegate( SyntaxTreeNode node )
      {
        // Negate = "-" + Spaces + Item;
        return new UnaryOperatorExpression( UnaryOperatorExpression.OperatorType.Negate, Convert( node.GetNamedChildren().First() ) );
      }

      private static IPropertyExpression ConvertVariable( SyntaxTreeNode node )
      {
        // Variable = VariableName + Spaces + ":=" + Spaces + SyntaxExpression.Placeholder( "ValueContext" ) + Spaces + ( ( "," + Spaces + SyntaxExpression.Placeholder( "ValueContext" ) ) | SyntaxExpression.Empty( "変数を適用する式がありません" ) ) ;
        var array = node.GetNamedChildren().ToArray() ;
        if ( array.Length != 3 ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidOperator );
        }

        return new VariableExpression( array[ 0 ].Value, Convert( array[ 1 ] ), Convert( array[ 2 ] ) ) ;
      }

      private static IPropertyExpression ConvertFunctionCall( SyntaxTreeNode node )
      {
        // FunctionCall = FunctionName + Spaces + "(" + Spaces + SyntaxExpression.Repeat( SyntaxExpression.Placeholder( "ValueContext" ) + Spaces, 0, "," + Spaces ) + ")";
        string funcName = null;
        var argList = new List<SyntaxTreeNode>();
        foreach ( var item in node.GetNamedChildren() ) {
          if ( null == funcName ) {
            funcName = item.Value;
            if ( !PropertyExpressionFunctionAttribute.ExistsFunction( funcName ) ) {
              throw new PropertyParseException( item.Text, item.StartIndex, PropertyParseErrorType.InvalidFunctionName );
            }
          }
          else {
            argList.Add( item );
          }
        }
        if ( null == funcName ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidFunctionName );
        }

        var funcInfo = PropertyExpressionFunctionAttribute.GetFunctionInfo( funcName );
        if ( argList.Count < funcInfo.ArgumentCount ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.ArgumentMissing, argList.Count, funcInfo.ArgumentCount );
        }
        if ( false == funcInfo.VariableLengthArgument ) {
          if ( argList.Count > funcInfo.ArgumentCount ) {
            throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.ArgumentTooMuch, argList.Count, funcInfo.ArgumentCount );
          }
        }

        return new FunctionOperatorExpression( funcName, Array.ConvertAll( argList.ToArray(), Convert ) );
      }

      private static IPropertyExpression ConvertMetaFunctionCall( SyntaxTreeNode node )
      {
        // MetaFunctionCall = MetaFunctionName + Spaces + "(" + Spaces + SyntaxExpression.Repeat( ObjectName + Spaces, 0, "," + Spaces ) + ")";
        string funcName = null;
        var argList = new List<SyntaxTreeNode>();
        foreach ( var item in node.GetNamedChildren() ) {
          if ( null == funcName ) {
            funcName = item.Value;
            if ( !PropertyExpressionMetaFunctionAttribute.ExistsMetaFunction( funcName ) ) {
              throw new PropertyParseException( item.Text, item.StartIndex, PropertyParseErrorType.InvalidFunctionName );
            }
          }
          else {
            argList.Add( item );
          }
        }
        if ( null == funcName ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidFunctionName );
        }

        var funcInfo = PropertyExpressionMetaFunctionAttribute.GetMetaFunctionInfo( funcName );
        if ( argList.Count < funcInfo.ArgumentCount ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.ArgumentMissing, argList.Count, funcInfo.ArgumentCount );
        }
        if ( false == funcInfo.VariableLengthArgument ) {
          if ( argList.Count > funcInfo.ArgumentCount ) {
            throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.ArgumentTooMuch, argList.Count, funcInfo.ArgumentCount );
          }
        }

        return new MetaFunctionOperatorExpression( funcName, Array.ConvertAll( argList.ToArray(), Convert ) );
      }

      private static IPropertyExpression ConvertObjectName( SyntaxTreeNode node )
      {
        return new SimpleObjectExpression( GetObjectName( node ) ) ;
      }

      private static IPropertyExpression ConvertProperty( SyntaxTreeNode node )
      {
        // Property = ObjectPropertyName | PropertyName;
        var children = node.GetNamedChildren().ToArray();
        if ( 1 == children.Length ) {
          return new ObjectPropertyExpression( null, children[0].Value );
        }
        if ( 2 == children.Length ) {
          return new ObjectPropertyExpression( GetObjectName( children[0] ), children[1].Value );
        }

        throw new InvalidProgramException();
      }

      private static string GetObjectName( SyntaxTreeNode child )
      {
        var value = child.Value ;
        if ( 0 < value.Length && '#' == value[0] ) {
          return value.Substring( 1 ) ;
        }

        return value ;
      }

      private static IPropertyExpression ConvertConstant( SyntaxTreeNode node )
      {
        // Constant = SyntaxExpression.FromRegExp( Join( "|", ConstantExpression.GetAllConstantNames() ), RegexOptions.IgnoreCase );
        return new ConstantExpression( node.Value );
      }

      private static IPropertyExpression ConvertNumericWithUnit( SyntaxTreeNode node )
      {
        // NumericWithUnit = Numeric + Spaces + Unit;
        var children = node.GetNamedChildren().ToArray();
        if ( 2 != children.Length ) {
          throw new InvalidProgramException();
        }

        double value;
        if ( false == double.TryParse( children[0].Value, out value ) ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidNumberFormat );
        }

        var unitName = children[1].Value;
        var unit = NumericExpression.GetUnit( unitName );
        if ( null == unit ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidUnitName );
        }

        return new NumericExpression( value, unit.Value );
      }
      private static IPropertyExpression ConvertNumeric( SyntaxTreeNode node )
      {
        // Numeric = SyntaxExpression.FromRegExp( @"[0-9]+(?:\.[0-9]*)?(?:e[+-]?[0-9]+)?" );
        if ( false == double.TryParse( node.Value, out var value ) ) {
          throw new PropertyParseException( node.Text, node.StartIndex, PropertyParseErrorType.InvalidNumberFormat );
        }

        return new NumericExpression( value, NumericExpression.NumericUnit.None );
      }

      private static IPropertyExpression ConvertLtoRBinaryOperator( SyntaxTreeNode node )
      {
        var items = new List<IPropertyExpression>();
        var comparison = new List<SyntaxTreeNode>();

        int index = 0;
        foreach ( var child in node.GetNamedChildren() ) {
          if ( 0 == index % 2 ) {
            items.Add( Convert( child ) );
          }
          else {
            comparison.Add( child );
          }
          ++index;
        }

        var expression = items[0];
        for ( int i = 0 ; i < comparison.Count ; ++i ) {
          var opType = BinaryOperatorExpression.GetOperator( comparison[i].Value );
          if ( null == opType ) {
            throw new PropertyParseException( comparison[i].Text, comparison[i].StartIndex, PropertyParseErrorType.InvalidOperator );
          }
          expression = new BinaryOperatorExpression( opType.Value, expression, items[i + 1] );
        }

        return expression;
      }
    }
  }
}
