using System ;
using System.Collections.Generic ;
using System.Globalization ;
using System.Linq ;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;
using Component = Chiyoda.CAD.Model.Component ;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// プロパティ式で使用できる関数の一覧
  /// </summary>
  static class PropertyExpressionFunctions
  {
    /// <summary>円周率。</summary>
    [PropertyExpressionConstant( "PI" )]
    public static double PI = Math.PI ;

    /// <summary>自然対数の低。</summary>
    [PropertyExpressionConstant( "E" )]
    public static double E = Math.E ;


    /// <summary>
    /// 最小値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Min", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double Min( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 不足はパース時に確認

      double min = double.PositiveInfinity ;
      foreach ( var e in args ) {
        double d = e.GetValue( context, elm ) ;
        if ( d < min ) min = d ;
      }

      return min ;
    }

    /// <summary>
    /// 最大値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Max", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double Max( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 不足はパース時に確認

      double max = double.NegativeInfinity ;
      foreach ( var e in args ) {
        double d = e.GetValue( context, elm ) ;
        if ( max < d ) max = d ;
      }

      return max ;
    }

    /// <summary>
    /// 絶対値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Abs", ArgumentCount = 1 )]
    public static double Abs( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Abs( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// 自然対数値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Log", ArgumentCount = 1 )]
    public static double Log( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var x = args[ 0 ].GetValue( context, elm ) ;
      if ( x <= 0 ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"Log({x})" ) ;
      }

      return Math.Log( x ) ;
    }

    /// <summary>
    /// 常用対数値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Log10", ArgumentCount = 1 )]
    public static double Log10( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var x = args[ 0 ].GetValue( context, elm ) ;
      if ( x <= 0 ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"Log10({x})" ) ;
      }

      return Math.Log10( x ) ;
    }

    /// <summary>
    /// 任意底の対数値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "LogX", ArgumentCount = 2 )]
    public static double LogX( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      double k = args[ 0 ].GetValue( context, elm ), x = args[ 1 ].GetValue( context, elm ) ;
      if ( k <= 0 || k == 1 || x <= 0 ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"LogX({k}, {x})" ) ;
      }

      return Math.Log( x ) / Math.Log( k ) ;
    }

    /// <summary>
    /// ルートを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Sqrt", ArgumentCount = 1 )]
    public static double Sqrt( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var x = args[ 0 ].GetValue( context, elm ) ;
      if ( x < 0 ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"Sqrt({x})" ) ;
      }

      return Math.Sqrt( x ) ;
    }

    /// <summary>
    /// べき乗値を求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Pow", ArgumentCount = 2 )]
    public static double Pow( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      double k = args[ 0 ].GetValue( context, elm ), x = args[ 1 ].GetValue( context, elm ) ;
      if ( k == 0 && x == 0 ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"Pow({k}, {x})" ) ;
      }

      if ( k < 0 ) {
        if ( Math.Floor( x ) != x ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"Pow({k}, {x})" ) ;
        }

        double d = Math.Pow( -k, x ) ;
        if ( Math.Floor( x / 2 ) != x / 2 ) d = -d ;
        return d ;
      }

      return Math.Pow( k, x ) ;
    }

    /// <summary>
    /// Sinを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Sin", ArgumentCount = 1 )]
    public static double Sin( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Sin( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// Cosを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Cos", ArgumentCount = 1 )]
    public static double Cos( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Cos( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// Tanを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Tan", ArgumentCount = 1 )]
    public static double Tan( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Tan( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// ASinを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "ASin", ArgumentCount = 1 )]
    public static double ASin( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      double x = args[ 0 ].GetValue( context, elm ) ;
      if ( x < -1 || 1 < x ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"ASin({x})" ) ;
      }

      return Math.Asin( x ) ;
    }

    /// <summary>
    /// Acosを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "ACos", ArgumentCount = 1 )]
    public static double ACos( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var x = args[ 0 ].GetValue( context, elm ) ;
      if ( x < -1 || 1 < x ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.DomainError, $"ACos({x})" ) ;
      }

      return Math.Acos( x ) ;
    }

    /// <summary>
    /// ATanを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "ATan", ArgumentCount = 1 )]
    public static double ATan( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Atan( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// 2引数版のATanを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "ATan2", ArgumentCount = 2 )]
    public static double ATan2( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Math.Atan2( args[ 0 ].GetValue( context, elm ), args[ 1 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// RadianをDegreeに変換します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Rad2Deg", ArgumentCount = 1 )]
    public static double Rad2Deg( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return args[ 0 ].GetValue( context, elm ).Rad2Deg() ;
    }

    /// <summary>
    /// DegreeをRadianに変換します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Deg2Rad", ArgumentCount = 1 )]
    public static double Deg2Rad( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return args[ 0 ].GetValue( context, elm ).Deg2Rad() ;
    }

    /// <summary>
    /// ベクトルの長さを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Hypot", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double Hypot( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var values = Array.ConvertAll( args, e => e.GetValue( context, elm ) ) ;
      Array.Sort( values ) ;

      double sum = 0 ;
      foreach ( var d in values ) sum += d * d ;

      return Math.Sqrt( sum ) ;
    }

    /// <summary>
    /// 1つめの条件によって条件分岐します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "If", ArgumentCount = 3 )]
    public static double If( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 3 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      if ( 0 != args[ 0 ].GetValue( context, elm ) ) {
        return args[ 1 ].GetValue( context, elm ) ;
      }
      else {
        return args[ 2 ].GetValue( context, elm ) ;
      }
    }

    /// <summary>
    /// 全て0以外である場合は1、そうでなければ0を返します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "And", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double And( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      foreach ( var arg in args ) {
        if ( 0 == arg.GetValue( context, elm ) ) return 0 ;
      }

      return 1 ;
    }

    /// <summary>
    /// 1つでも0以外である場合はその値、そうでなければ0を返します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Or", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double Or( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      foreach ( var arg in args ) {
        double x = arg.GetValue( context, elm ) ;
        if ( 0 != x ) return x ;
      }

      return 0 ;
    }

    /// <summary>
    /// 引数が0以外の場合は0、そうでなければ1を返します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Not", ArgumentCount = 1 )]
    public static double Not( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return ( 0 == args[ 0 ].GetValue( context, elm ) ) ? 1 : 0 ;
    }

    /// <summary>
    /// 引数が0の場合は0、そうでなければ1を返します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Bool", ArgumentCount = 1 )]
    public static double Bool( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return ( 0 == args[ 0 ].GetValue( context, elm ) ) ? 0 : 1 ;
    }

    /// <summary>
    /// 式が実行時エラーとなるかどうかを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "IsError", ArgumentCount = 1 )]
    public static double IsError( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      try {
        args[ 0 ].GetValue( context, elm ) ;
      }
      catch ( PropertyValidationAbortedException ) {
        return 0 ;
      }
      catch ( PropertyRuntimeException ) {
        return 1 ;
      }

      return 0 ;
    }

    /// <summary>
    /// 式が実行時エラーとならない最初の値を取得します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Valid", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double Valid( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      PropertyRuntimeException lastError = null ;
      foreach ( var arg in args ) {
        try {
          return arg.GetValue( context, elm ) ;
        }
        catch ( PropertyRuntimeException ex ) {
          lastError = ex ;
        }
        catch ( PropertyValidationAbortedException ex ) {
          throw ;
        }
      }

      throw lastError ;
    }


    /// <summary>
    /// 式の評価を中断し、プロパティの設定をキャンセルします。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "Abort", ArgumentCount = 0 )]
    public static double Abort( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      throw new PropertyValidationAbortedException() ;
    }

    /// <summary>
    /// 式の評価が中断されるかどうかを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "IsAborted", ArgumentCount = 1 )]
    public static double IsAborted( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      try {
        args[ 0 ].GetValue( context, elm ) ;
      }
      catch ( PropertyValidationAbortedException ) {
        return 1 ;
      }
      catch ( PropertyRuntimeException ) {
        return 1 ;
      }

      return 0 ;
    }

    /// <summary>
    /// 式が実行中断とならない最初の値を取得します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "NonAborted", VariableLengthArgument = true, ArgumentCount = 1 )]
    public static double NonAborted( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      PropertyValidationAbortedException lastAborted = null ;
      foreach ( var arg in args ) {
        try {
          return arg.GetValue( context, elm ) ;
        }
        catch ( PropertyValidationAbortedException ex ) {
          lastAborted = ex ;
        }
      }

      throw lastAborted ;
    }

    /// <summary>
    /// 指定直径のElbow90の、軸からConnectPointまでの長さを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "DiameterToElbow90Length", ArgumentCount = 1 )]
    public static double DiameterToElbow90Length( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var diameter = DiameterFactory.FromOutsideMeter( args[ 0 ].GetValue( context, elm ) ) ;
      return PipingElbow90.GetDefaultLongBendLength( diameter ) ;
    }

    /// <summary>
    /// 指定直径のPipingTeeの、主軸の長さを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "DiameterToTeeMainLength", ArgumentCount = 2 )]
    public static double DiameterToTeeMainLength( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var diameterMain = DiameterFactory.FromOutsideMeter( args[ 0 ].GetValue( context, elm ) ) ;
      var diameterBranch = DiameterFactory.FromOutsideMeter( args[ 1 ].GetValue( context, elm ) ) ;
      return PipingTee.GetDefaultMainLength( diameterMain, diameterBranch ) ;
    }

    /// <summary>
    /// 指定直径のPipingTeeの、主軸から枝ConnectPointまでの長さを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "DiameterToTeeBranchLength", ArgumentCount = 2 )]
    public static double DiameterToTeeBranchLength( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var diameterMain = DiameterFactory.FromOutsideMeter( args[ 0 ].GetValue( context, elm ) ) ;
      var diameterBranch = DiameterFactory.FromOutsideMeter( args[ 1 ].GetValue( context, elm ) ) ;
      return PipingTee.GetDefaultBranchLength( diameterMain, diameterBranch ) ;
    }

    /// <summary>
    /// 指定直径のPipeの、DB指定された最小長さを求めます。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "DiameterToPipeMinLength", ArgumentCount = 1 )]
    public static double DiameterToMinPipeLength( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      return Pipe.GetDefaultPipeMinLength( args[ 0 ].GetValue( context, elm ) ) ;
    }

    /// <summary>
    /// 指定の値をログ出力し、そのまま返します。
    /// </summary>
    /// <param name="context">式コンテキスト</param>
    /// <param name="elm">式が定義された要素</param>
    /// <param name="args">引数</param>
    /// <returns>戻り値</returns>
    [PropertyExpressionFunction( "DebugLog", ArgumentCount = 1 )]
    public static double DebugLog( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
    {
      if ( 1 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

      var value = args[ 0 ].GetValue( context, elm ) ;
      Debug.Log( $"RuleExpression: {value} <-- {args[ 0 ]}" ) ;
      return value ;
    }


    private abstract class DistanceOfBase : IMetaFunction
    {
      protected DistanceOfBase( string[] propNames )
      {
        PropertyNames = propNames ;
      }

      private string[] PropertyNames { get ; }

      public double GetValue( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
      {
        if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

        var elm1 = args[ 0 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 0 ].ToString() ) ;
        var elm2 = args[ 1 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 1 ].ToString() ) ;

        var obj1 = elm1.GetObject( elm ) ;
        if ( null == obj1 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        var obj2 = elm2.GetObject( elm ) ;
        if ( null == obj2 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        if ( obj1.Parent != obj2.Parent ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ObjectParentDifferent, obj1.ObjectName, obj2.ObjectName ) ;
        }

        if ( 1 == PropertyNames.Length ) {
          var name = PropertyNames[ 0 ] ;
          return Math.Abs( obj1.GetProperty( name ).Value - obj2.GetProperty( name ).Value ) ;
        }

        double r2 = 0 ;
        foreach ( var name in PropertyNames ) {
          var diff = obj1.GetProperty( name ).Value - obj2.GetProperty( name ).Value ;
          r2 += diff * diff ;
        }

        return Math.Sqrt( r2 ) ;
      }

      public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm, IPropertyExpression[] args )
      {
        if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

        var elm1 = args[ 0 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 0 ].ToString() ) ;
        var elm2 = args[ 1 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 1 ].ToString() ) ;

        var obj1 = elm1.GetObject( elm ) ;
        if ( null == obj1 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        var obj2 = elm2.GetObject( elm ) ;
        if ( null == obj2 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 1 ].ToString() ) ;
        }

        foreach ( var propName in PropertyNames ) {
          var prop = obj1.GetProperty( propName ) ;
          if ( null != prop ) {
            yield return new KeyValuePair<IPropertiedElement, INamedProperty>( obj1, prop ) ;
          }
        }

        foreach ( var propName in PropertyNames ) {
          var prop = obj2.GetProperty( propName ) ;
          if ( null != prop ) {
            yield return new KeyValuePair<IPropertiedElement, INamedProperty>( obj2, prop ) ;
          }
        }
      }
    }

    /// <summary>
    /// 指定オブジェクトの中心間X距離を求めます。
    /// </summary>
    [PropertyExpressionMetaFunction( "DistanceXOf", ArgumentCount = 2 )]
    private class DistanceXOf : DistanceOfBase
    {
      public DistanceXOf() : base( new[] { "PosX" } )
      {
      }
    }

    /// <summary>
    /// 指定オブジェクトの中心間Y距離を求めます。
    /// </summary>
    [PropertyExpressionMetaFunction( "DistanceYOf", ArgumentCount = 2 )]
    private class DistanceYOf : DistanceOfBase
    {
      public DistanceYOf() : base( new[] { "PosY" } )
      {
      }
    }

    /// <summary>
    /// 指定オブジェクトの中心間Z距離を求めます。
    /// </summary>
    [PropertyExpressionMetaFunction( "DistanceZOf", ArgumentCount = 2 )]
    private class DistanceZOf : DistanceOfBase
    {
      public DistanceZOf() : base( new[] { "PosZ" } )
      {
      }
    }

    /// <summary>
    /// 指定オブジェクトの中心間平面距離を求めます。
    /// </summary>
    [PropertyExpressionMetaFunction( "DistanceXYOf", ArgumentCount = 2 )]
    private class DistanceXYOf : DistanceOfBase
    {
      public DistanceXYOf() : base( new[] { "PosX", "PosY" } )
      {
      }
    }

    /// <summary>
    /// 指定オブジェクトの中心間立体距離を求めます。
    /// </summary>
    [PropertyExpressionMetaFunction( "DistanceXYZOf", ArgumentCount = 2 )]
    private class DistanceXYZOf : DistanceOfBase
    {
      public DistanceXYZOf() : base( new[] { "PosX", "PosY", "PosZ" } )
      {
      }
    }

    private abstract class MinHorzDistanceOfBase : IMetaFunction
    {
      public double GetValue( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
      {
        if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

        var elm1 = args[ 0 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 0 ].ToString() ) ;
        var elm2 = args[ 1 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 1 ].ToString() ) ;

        var obj1 = elm1.GetObject( elm ) ;
        if ( null == obj1 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        var obj2 = elm2.GetObject( elm ) ;
        if ( null == obj2 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        if ( obj1.Parent != obj2.Parent ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ObjectParentDifferent, obj1.ObjectName, obj2.ObjectName ) ;
        }

        return GetAllComponents( obj1, obj2 ).Select( v => GetMinLength( v.Item1, v.Item2, v.Item3 ) ).Sum() ;
      }

      public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm, IPropertyExpression[] args )
      {
        if ( 2 != args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

        var elm1 = args[ 0 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 0 ].ToString() ) ;
        var elm2 = args[ 1 ] as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args[ 1 ].ToString() ) ;

        var obj1 = elm1.GetObject( elm ) ;
        if ( null == obj1 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 0 ].ToString() ) ;
        }

        var obj2 = elm2.GetObject( elm ) ;
        if ( null == obj2 ) {
          throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, args[ 1 ].ToString() ) ;
        }

        return GetAllComponents( obj1, obj2 ).Select( x => x.Item1 ).SelectMany( GetSources ) ;
      }

      private static IEnumerable<(PipingPiece, int, int)> GetAllComponents( IPropertiedElement obj1, IPropertiedElement obj2 )
      {
        var le1 = GetLeafEdge( obj1 ) ;
        var le2 = GetLeafEdge( obj2 ) ;

        var globalCod = le1.GlobalCod ;
        foreach ( var v in le1.Vertices ) {
          var dir = globalCod.GlobalizeVector( v.GetConnectVector() ) ;
          if ( ! dir.IsPerpendicularTo( Vector3d.forward ) ) continue ;

          var pp = GetAllComponents( v, le2 ) ;
          if ( null != pp ) return pp ;
        }

        return Array.Empty<(PipingPiece, int, int)>() ;
      }

      private static LeafEdge GetLeafEdge( IPropertiedElement elm )
      {
        switch ( elm ) {
          case LeafEdge le : return le ;
          case PipingPiece pp : return pp.LeafEdge ;
          default : throw new PropertyRuntimeException( PropertyRuntimeErrorType.BadElement, elm.ToString() ) ;
        }
      }

      private static IEnumerable<(PipingPiece, int, int)> GetAllComponents( HalfVertex v, LeafEdge le )
      {
        if ( null == v.LeafEdge.PipingPiece || null == le.PipingPiece ) return null ;

        var list = new List<(PipingPiece, int, int)> { ( v.LeafEdge.PipingPiece, -1, v.ConnectPointIndex ) } ;
        while ( true ) {
          var partner = v.Partner ;
          var edge = partner?.LeafEdge ;
          if ( null == edge ) return null ;

          var connectNumber = partner.ConnectPointIndex ;

          if ( edge == le ) {
            list.Add( ( le.PipingPiece, connectNumber, -1 ) ) ;
            return list ;
          }

          if ( ! ( edge.PipingPiece is Component pp ) ) return null ;

          var cp = pp.GetAntiPoleConnectPoint( connectNumber ) ;
          if ( null == cp ) return null ;

          var anotherNumber = cp.ConnectPointNumber ;
          list.Add( ( pp, connectNumber, anotherNumber ) ) ;
          v = edge.GetVertex( anotherNumber ) ;
        }
      }

      private protected abstract double GetMinLength( PipingPiece pp, int fromIndex1, int fromIndex2 ) ;

      private protected abstract IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSources( PipingPiece pp ) ;
    }

    [PropertyExpressionMetaFunction( "MinHorzDistanceOf", ArgumentCount = 2 )]
    private class MinHorzDistanceOf : MinHorzDistanceOfBase
    {
      private protected override double GetMinLength( PipingPiece pp, int fromIndex1, int fromIndex2 )
      {
        switch ( pp ) {
          case Pipe pipe :
            return ( 0 == pipe.LeafEdge.GetFlexRatio() ? pipe.Length : pipe.GetMinimumLength() ) * ( ( 0 <= fromIndex1 && 0 <= fromIndex2 ) ? 1.0 : 0.5 ) ;

          case WeldOlet _ :
          case StubInReinforcingWeld _ :
            return 0 ;

          case ILinearComponent comp :
            return comp.Length * ( ( 0 <= fromIndex1 && 0 <= fromIndex2 ) ? 1.0 : 0.5 ) ;
        }

        var len1 = ( 0 <= fromIndex1 ) ? pp.GetConnectPoint( fromIndex1 ).Point.magnitude : 0.0 ;
        var len2 = ( 0 <= fromIndex2 ) ? pp.GetConnectPoint( fromIndex2 ).Point.magnitude : 0.0 ;
        return len1 + len2 ;
      }

      private protected override IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSources( PipingPiece pp )
      {
        if ( pp is Pipe pipe ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, pipe.GetProperty( "MinLength" ) ) ;
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, pipe.GetProperty( "Length" ) ) ;
        }
        else if ( pp is WeldOlet wo ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, wo.GetProperty( "Diameter" ) ) ;
        }
        else if ( pp is StubInReinforcingWeld sw ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, sw.GetProperty( "Diameter" ) ) ;
        }
        else {
          var prop = pp.GetProperty( "Length" ) ;
          if ( null != prop ) {
            yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, prop ) ;
          }
        }
      }
    }

    [PropertyExpressionMetaFunction( "SystemMinHorzDistanceOf", ArgumentCount = 2 )]
    private class SystemMinHorzDistanceOf : MinHorzDistanceOfBase
    {
      private protected override double GetMinLength( PipingPiece pp, int fromIndex1, int fromIndex2 )
      {
        switch ( pp ) {
          case Pipe pipe :
            return pipe.GetMinimumLength() * ( ( 0 <= fromIndex1 && 0 <= fromIndex2 ) ? 1.0 : 0.5 ) ;

          case WeldOlet _ :
          case StubInReinforcingWeld _ :
            return 0 ;
          
          case ILinearComponent comp :
            return comp.Length * ( ( 0 <= fromIndex1 && 0 <= fromIndex2 ) ? 1.0 : 0.5 ) ;
        }

        var len1 = ( 0 <= fromIndex1 ) ? pp.GetConnectPoint( fromIndex1 ).Point.magnitude : 0.0 ;
        var len2 = ( 0 <= fromIndex2 ) ? pp.GetConnectPoint( fromIndex2 ).Point.magnitude : 0.0 ;
        return len1 + len2 ;
      }

      private protected override IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSources( PipingPiece pp )
      {
        if ( pp is Pipe pipe ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, pipe.GetProperty( "MinLength" ) ) ;
        }
        else if ( pp is WeldOlet wo ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, wo.GetProperty( "Diameter" ) ) ;
        }
        else if ( pp is StubInReinforcingWeld sw ) {
          yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, sw.GetProperty( "Diameter" ) ) ;
        }
        else {
          var prop = pp.GetProperty( "Length" ) ;
          if ( null != prop ) {
            yield return new KeyValuePair<IPropertiedElement, INamedProperty>( pp, prop ) ;
          }
        }
      }
    }

    [PropertyExpressionMetaFunction( "ExistsElement", ArgumentCount = 1, VariableLengthArgument = true )]
    private class ExistsElement : IMetaFunction
    {
      public double GetValue( PropertyExpressionContext context, IPropertiedElement elm, IPropertyExpression[] args )
      {
        if ( 0 == args.Length ) throw new PropertyRuntimeException( PropertyRuntimeErrorType.UnknownError ) ; // 引数数不一致はパース時に確認

        foreach ( var arg in args ) {
          var a = arg as ObjectExpression ?? throw new PropertyRuntimeException( PropertyRuntimeErrorType.NotElement, args.ToString() ) ;
          var obj = a.GetObject( elm ) ;
          if ( null == obj ) return 0 ;
        }

        return 1 ;
      }

      public IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetSourceProperties( IPropertiedElement elm, IPropertyExpression[] args )
      {
        return Array.Empty<KeyValuePair<IPropertiedElement, INamedProperty>>() ;
      }
    }
  }
}