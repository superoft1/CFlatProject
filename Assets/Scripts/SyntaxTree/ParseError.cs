using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Chiyoda.SyntaxTree
{
  public abstract class SyntaxException : Exception
  {
    public static SyntaxException InfiniteDefinitionLoop( string expressionName, int pos )
    {
      return new SyntaxInfiniteDefinitionLoopException( expressionName, pos );
    }

    public static SyntaxException RequiresString( string expressionName, int pos, string requiredString )
    {
      return new SyntaxRequiresStringException( expressionName, pos, requiredString );
    }

    public static SyntaxException RequiresPattern( string expressionName, int pos, Regex requiredPattern )
    {
      return new SyntaxRequiresPatternException( expressionName, pos, requiredPattern );
    }

    public static SyntaxException RequiresAny( string expressionName, int pos, params SyntaxException[] allExceptions )
    {
      return new SyntaxRequiresAnyException( expressionName, pos, allExceptions );
    }

    public static SyntaxException RequiresRepeat( string expressionName, SyntaxException lastParseError, int minCount, int realCount )
    {
      return new SyntaxRequiresRepeatException( expressionName, lastParseError, minCount, realCount );
    }

    public static SyntaxException SyntaxError( string expressionName, int pos, string errorMessage )
    {
      return new SyntaxErrorException( expressionName, pos, errorMessage );
    }


    public string ExpressionName { get; }
    public int ErrorPosition { get; }

    protected SyntaxException( string expressionName, int pos, string message )
      : base( string.IsNullOrEmpty( expressionName ) ? $"{pos} 文字目: {message}" : $"{pos} 文字目 ({expressionName}): {message}" )
    {
      ExpressionName = expressionName;
      ErrorPosition = pos;
    }
    protected SyntaxException( SerializationInfo info, StreamingContext context ) : base( info, context )
    {
      ExpressionName = info.GetString( "ExpressionName" );
      ErrorPosition = info.GetInt32( "ErrorPosition" );
    }

    public override void GetObjectData( SerializationInfo info, StreamingContext context )
    {
      base.GetObjectData( info, context );

      info.AddValue( "ExpressionName", ExpressionName );
      info.AddValue( "ErrorPosition", ErrorPosition );
    }



    private class SyntaxInfiniteDefinitionLoopException : SyntaxException
    {
      public string RequiredString { get; private set; }

      public SyntaxInfiniteDefinitionLoopException( string expressionName, int pos )
        : base( expressionName, pos, "定義が無限ループしています。構文解析を巻き戻します。" )
      {
      }

      protected SyntaxInfiniteDefinitionLoopException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );
      }
    }

    private class SyntaxRequiresStringException : SyntaxException
    {
      public string RequiredString { get; private set; }

      public SyntaxRequiresStringException( string expressionName, int pos, string requiredString )
        : base( expressionName, pos, $"文字列 \"{requiredString}\" が必要です。" )
      {
        if ( null == requiredString ) throw new ArgumentNullException( nameof( requiredString ) );

        RequiredString = requiredString;
      }

      protected SyntaxRequiresStringException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
        RequiredString = info.GetString( "RequiredString" );
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );

        info.AddValue( "RequiredString", RequiredString );
      }
    }

    private class SyntaxRequiresPatternException : SyntaxException
    {
      public Regex RequiredPattern { get; private set; }

      public SyntaxRequiresPatternException( string expressionName, int pos, Regex requiredPattern )
        : base( expressionName, pos, $"パターン \"{requiredPattern.ToString()}\" が必要です。" )
      {
        if ( null == requiredPattern ) throw new ArgumentNullException( nameof( requiredPattern ) );

        RequiredPattern = requiredPattern;
      }

      protected SyntaxRequiresPatternException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
        RequiredPattern = new Regex( info.GetString( "RequiredPattern" ), (RegexOptions)info.GetInt32( "RegexOptions" ) );
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );

        info.AddValue( "RequiredPattern", RequiredPattern.ToString() );
        info.AddValue( "RegexOptions", (int)RequiredPattern.Options );
      }
    }

    private class SyntaxRequiresAnyException : SyntaxException
    {
      private readonly SyntaxException[] _exceptions;

      public SyntaxException[] AllPatternUnmaches { get { return _exceptions.Clone() as SyntaxException[]; } }

      public SyntaxRequiresAnyException( string expressionName, int pos, SyntaxException[] allExceptions )
        : base( expressionName, pos, $"{allExceptions.Length} パターンのうち、いずれにもマッチしません。" )
      {
        _exceptions = allExceptions.Clone() as SyntaxException[];
      }

      protected SyntaxRequiresAnyException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
        _exceptions = info.GetValue( "AllPatternUnmaches", typeof( SyntaxException[] ) ) as SyntaxException[];
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );

        info.AddValue( "AllPatternUnmaches", AllPatternUnmaches, typeof( SyntaxException[] ) );
      }
    }

    private class SyntaxRequiresRepeatException : SyntaxException
    {
      public SyntaxException LastUnmatch { get; private set; }
      public int RequiredMinimumCount { get; private set; }
      public int RealCount { get; private set; }

      public SyntaxRequiresRepeatException( string expressionName, SyntaxException lastParseError, int minCount, int realCount )
        : base( expressionName, lastParseError.ErrorPosition, $"パターンの繰り返し数が {minCount} 回に足りません: {realCount} 回" )
      {
        LastUnmatch = lastParseError;
        RequiredMinimumCount = minCount;
        RealCount = realCount;
      }

      protected SyntaxRequiresRepeatException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
        LastUnmatch = info.GetValue( "LastUnmatch", typeof( SyntaxException ) ) as SyntaxException;
        RequiredMinimumCount = info.GetInt32( "RequiredMinimumCount" );
        RealCount = info.GetInt32( "RealCount" );
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );

        info.AddValue( "LastUnmatch", LastUnmatch, typeof( SyntaxException ) );
        info.AddValue( "RequiredMinimumCount", RequiredMinimumCount );
        info.AddValue( "RealCount", RealCount );
      }
    }

    private class SyntaxErrorException : SyntaxException
    {
      public SyntaxErrorException( string expressionName, int pos, string errorMessage )
        : base( expressionName, pos, errorMessage )
      {
      }

      protected SyntaxErrorException( SerializationInfo info, StreamingContext context ) : base( info, context )
      {
      }

      public override void GetObjectData( SerializationInfo info, StreamingContext context )
      {
        base.GetObjectData( info, context );
      }
    }
  }
}