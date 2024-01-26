using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// プロパティ式の解釈失敗のタイプです。
  /// </summary>
  public enum PropertyParseErrorType
  {
    /// <summary>未知の解釈失敗タイプ</summary>
    UnknownError,

    /// <summary>演算子が正しくありません。</summary>
    InvalidOperator,
    /// <summary>関数名が正しくありません。</summary>
    InvalidFunctionName,
    /// <summary>引数が足りません。</summary>
    ArgumentMissing,
    /// <summary>引数が多すぎます。</summary>
    ArgumentTooMuch,
    /// <summary>定数名が正しくありません。</summary>
    InvalidConstantName,
    /// <summary>数値フォーマットが正しくありません。</summary>
    InvalidNumberFormat,
    /// <summary>単位が正しくありません。</summary>
    InvalidUnitName,

    /// <summary>プロパティ指定が正しくありません。</summary>
    InvalidPropertyExpression,
  }

  /// <summary>
  /// プロパティ式の解釈に失敗した際の例外です。
  /// </summary>
  public class PropertyParseException : Exception
  {
    private readonly object[] _errorArgs;

    public string Expression { get; }
    public int CharIndex { get; }
    public PropertyParseErrorType ErrorCode { get; }
    public object[] ErrorArguments => _errorArgs.Clone() as object[] ;

    public PropertyParseException( string expression, int charIndex, PropertyParseErrorType errorCode )
      : this( expression, charIndex, errorCode, null )
    { }
    public PropertyParseException( string expression, int charIndex, PropertyParseErrorType errorCode, params object[] errorArgs )
      : base( GetErrorMessage( expression, charIndex, errorCode ) )
    {
      Expression = expression;
      CharIndex = charIndex;
      ErrorCode = errorCode;
      _errorArgs = errorArgs ?? Array.Empty<object>();
    }

    public PropertyParseException( string expression, SyntaxTree.SyntaxException syntaxException )
      : base( CombineErrorMessage( syntaxException.Message, expression, syntaxException.ErrorPosition ) )
    {
      Expression = expression ;
      CharIndex = syntaxException.ErrorPosition ;
      ErrorCode = PropertyParseErrorType.UnknownError ;
      _errorArgs = Array.Empty<object>() ;
    }

    protected PropertyParseException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
      Expression = info.GetString( "Expression" );
      CharIndex = info.GetInt32( "CharIndex" );
      try {
        ErrorCode = (PropertyParseErrorType)Enum.Parse( typeof( PropertyParseErrorType ), info.GetString( "ErrorCode" ) );
      }
      catch ( ArgumentException ) {
        ErrorCode = PropertyParseErrorType.UnknownError;
      }

      _errorArgs = info.GetValue( "ErrorArguments", typeof( object[] ) ) as object[];
    }

    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
      base.GetObjectData( info, context );

      info.AddValue( "Expression", Expression );
      info.AddValue( "CharIndex", CharIndex );
      info.AddValue( "ErrorCode", ErrorCode.ToString() );
      info.AddValue( "ErrorArguments", _errorArgs );
    }

    private static string GetErrorMessage( string expression, int charIndex, PropertyParseErrorType errorType )
    {
      return CombineErrorMessage( GetErrorMessageHead( errorType ), expression, charIndex ) ;
    }

    private static string CombineErrorMessage( string messageHead, string expression, int charIndex )
    {
      const int SHOW_LENGTH = 10 ;
      
      if ( SHOW_LENGTH + 3 <= charIndex ) {
        expression = "..." + expression.Substring( charIndex - SHOW_LENGTH ) ;
        charIndex = SHOW_LENGTH + 3 ;
      }

      if ( charIndex + (SHOW_LENGTH + 4) < expression.Length ) {
        expression = expression.Substring( 0, charIndex + (SHOW_LENGTH + 1) ) + "..." ;
      }

      return $"{messageHead}\r\n{expression}\r\n{new string( '_', charIndex )}^" ;
    }

    private static string GetErrorMessageHead( PropertyParseErrorType errorType )
    {
      switch ( errorType ) {
        case PropertyParseErrorType.InvalidOperator: return "演算子が正しくありません。" ;
        case PropertyParseErrorType.InvalidFunctionName: return "関数名が正しくありません。" ;
        case PropertyParseErrorType.ArgumentMissing: return "引数が足りません。" ;
        case PropertyParseErrorType.ArgumentTooMuch: return "引数が多すぎます。" ;
        case PropertyParseErrorType.InvalidConstantName: return "定数名が正しくありません。" ;
        case PropertyParseErrorType.InvalidNumberFormat: return "数値フォーマットが正しくありません。" ;
        case PropertyParseErrorType.InvalidUnitName: return "単位が正しくありません。" ;
        case PropertyParseErrorType.InvalidPropertyExpression: return "プロパティ指定が正しくありません。" ;

        default: return "未知の解釈失敗タイプです。" ;
      }
    }
  }
}
