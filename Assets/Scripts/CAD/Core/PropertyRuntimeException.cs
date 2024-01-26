using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// プロパティ式の実行時エラーのタイプです。
  /// </summary>
  public enum PropertyRuntimeErrorType
  {
    /// <summary>未知の実行時エラータイプ</summary>
    UnknownError,

    /// <summary>0で除算しました。</summary>
    DividedByZero,
    /// <summary>オブジェクトの親が異なっています。</summary>
    ObjectParentDifferent,
    /// <summary>関数の定義域外です。</summary>
    DomainError,

    /// <summary>要素が見つかりません。</summary>
    ElementNotFound,
    /// <summary>オブジェクトではありません。</summary>
    NotElement,
    /// <summary>要素が正しくありません。</summary>
    BadElement,
    /// <summary>プロパティが見つかりません。</summary>
    PropertyNotFound,
    
    /// <summary>定数・変数が見つかりません。</summary>
    VariableNotFound,
  }

  /// <summary>
  /// プロパティ式の解釈に失敗した際の例外です。
  /// </summary>
  public class PropertyRuntimeException : Exception
  {
    private readonly string[] _errorArgs;

    public PropertyRuntimeErrorType ErrorCode { get; private set; }
    public string[] ErrorArguments { get { return _errorArgs.Clone() as string[]; } }

    public PropertyRuntimeException( PropertyRuntimeErrorType errorCode )
      : this( errorCode, null )
    { }

    public PropertyRuntimeException( PropertyRuntimeErrorType errorCode, params string[] errorArgs )
      : base( ( null != errorArgs && 0 < errorArgs.Length ) ? $"{errorCode}: {string.Join( ", ", errorArgs )}" : errorCode.ToString() )
    {
      ErrorCode = errorCode ;
      _errorArgs = errorArgs ?? Array.Empty<string>() ;
    }

    protected PropertyRuntimeException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
      try {
        ErrorCode = (PropertyRuntimeErrorType)Enum.Parse( typeof( PropertyRuntimeErrorType ), info.GetString( "ErrorCode" ) );
      }
      catch ( ArgumentException ) {
        ErrorCode = PropertyRuntimeErrorType.UnknownError;
      }

      _errorArgs = info.GetValue( "ErrorArguments", typeof( string[] ) ) as string[];
    }

    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
      base.GetObjectData( info, context );

      info.AddValue( "ErrorCode", ErrorCode.ToString() );
      info.AddValue( "ErrorArguments", _errorArgs );
    }
  }

  public class PropertyValidationAbortedException : Exception
  {
    public PropertyValidationAbortedException()
    { }
    protected PropertyValidationAbortedException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }
  }

  public class PropertyMustBeRevalidatedException : Exception
  {
    public PropertyMustBeRevalidatedException()
    { }
    protected PropertyMustBeRevalidatedException( SerializationInfo info, StreamingContext context )
      : base( info, context )
    {
    }
  }
}
