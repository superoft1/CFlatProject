using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chiyoda.SyntaxTree
{
  using ExpressionPositionPair = System.Collections.Generic.KeyValuePair<SyntaxExpression, int>;

  public abstract class SyntaxExpression
  {
    private string _name = null;

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public static implicit operator SyntaxExpression( string str )
    {
      return FromString( str );
    }

    public static SyntaxExpression Empty( string errorString )
    {
      return new ErrorOnEmptyExpression( errorString );
    }
    public static SyntaxExpression FromString( string str )
    {
      return new NormalExpression( str );
    }
    public static SyntaxExpression FromRegExp( string pattern )
    {
      return new RegexExpression( pattern, RegexOptions.None );
    }
    public static SyntaxExpression FromRegExp( string pattern, RegexOptions options )
    {
      return new RegexExpression( pattern, options );
    }
    public static SyntaxExpression FromRegExp( Regex regex )
    {
      return new RegexExpression( regex );
    }
    public static SyntaxExpression Placeholder( string name )
    {
      if ( string.IsNullOrEmpty( name ) ) throw new ArgumentNullException( nameof( name ) );
      return new PlaceholderExpression( name );
    }
    public static SyntaxExpression Repeat( SyntaxExpression exp, int minCount, SyntaxExpression delimiter )
    {
      return new RepeatExpression( exp, minCount, int.MaxValue, delimiter );
    }
    public static SyntaxExpression Repeat( SyntaxExpression exp, int minCount, int maxCount, SyntaxExpression delimiter )
    {
      return new RepeatExpression( exp, minCount, maxCount, delimiter );
    }
    public static SyntaxExpressionJoiner operator +( SyntaxExpression ex1, SyntaxExpression ex2 )
    {
      return new SyntaxExpressionJoiner( new[] { ex1, ex2 }, SyntaxExpressionJoiner.JoinType.Serial );
    }
    public static SyntaxExpressionJoiner operator |( SyntaxExpression ex1, SyntaxExpression ex2 )
    {
      return new SyntaxExpressionJoiner( new[] { ex1, ex2 }, SyntaxExpressionJoiner.JoinType.Parallel );
    }
    public static SyntaxExpression NameOf( Regex pattern, IEnumerable<string> allowedNames, string messageOnNotFound )
    {
      return new NameExpression( pattern, allowedNames, messageOnNotFound );
    }

    public virtual string PlaceholderName { get { return null; } }

    public abstract void ReplacePlaceholder( string name, SyntaxExpression expression );

    public SyntaxTreeNode Parse( string text )
    {
      Context context = new Context( text );
      var tree = Parse( context );

      if ( null != context.SyntaxException ) {
        throw context.SyntaxException ;
      }

      if ( text.Length != context.CurrentPosition ) {
        throw SyntaxException.SyntaxError( null, context.CurrentPosition, "式が正しくありません: " + GetNextToken( text, context.CurrentPosition ) ) ;
      }
      return tree;
    }

    private static readonly Regex Token = new Regex( $@"[A-Za-z0-9]+", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase ) ;
    private const int MinTokenLength = 5 ;

    private static string GetNextToken( string text, int position )
    {
      if ( text.Length < position + MinTokenLength ) {
        return text.Substring( position ) ;
      }

      var match = Token.Match( text, position + MinTokenLength - 1 ) ;
      if ( match.Success ) {
        return text.Substring( position, MinTokenLength - 1 ) + match.Value ;
      }
      
      return text.Substring( position, MinTokenLength ) ;
    }

    protected abstract SyntaxTreeNode Parse( Context context );


    public class SyntaxExpressionJoiner
    {
      public enum JoinType
      {
        Serial,
        Parallel,
      }

      private readonly SyntaxExpression[] _expressions;
      private readonly JoinType _joinType;

      public SyntaxExpressionJoiner( SyntaxExpression[] expressions, JoinType joinType )
      {
        _expressions = expressions;
        _joinType = joinType;
      }

      public static SyntaxExpressionJoiner operator +( SyntaxExpressionJoiner ex1, SyntaxExpression ex2 )
      {
        return Join( ex1, ex2, JoinType.Serial );
      }
      public static SyntaxExpressionJoiner operator +( SyntaxExpression ex1, SyntaxExpressionJoiner ex2 )
      {
        return Join( ex1, ex2, JoinType.Serial );
      }
      public static SyntaxExpressionJoiner operator +( SyntaxExpressionJoiner ex1, SyntaxExpressionJoiner ex2 )
      {
        return Join( ex1, ex2, JoinType.Serial );
      }

      public static SyntaxExpressionJoiner operator |( SyntaxExpressionJoiner ex1, SyntaxExpression ex2 )
      {
        return Join( ex1, ex2, JoinType.Parallel );
      }
      public static SyntaxExpressionJoiner operator |( SyntaxExpression ex1, SyntaxExpressionJoiner ex2 )
      {
        return Join( ex1, ex2, JoinType.Parallel );
      }
      public static SyntaxExpressionJoiner operator |( SyntaxExpressionJoiner ex1, SyntaxExpressionJoiner ex2 )
      {
        return Join( ex1, ex2, JoinType.Parallel );
      }

      private static SyntaxExpressionJoiner Join( SyntaxExpressionJoiner ex1, SyntaxExpression ex2, JoinType joinType )
      {
        SyntaxExpression[] array;
        if ( joinType == ex1._joinType ) {
          array = new SyntaxExpression[ex1._expressions.Length + 1];
          Array.Copy( ex1._expressions, array, ex1._expressions.Length );
          array[ex1._expressions.Length] = ex2;
        }
        else {
          array = new[] { (SyntaxExpression)ex1, ex2 };
        }

        return new SyntaxExpressionJoiner( array, joinType );
      }
      private static SyntaxExpressionJoiner Join( SyntaxExpression ex1, SyntaxExpressionJoiner ex2, JoinType joinType )
      {
        SyntaxExpression[] array;
        if ( joinType == ex2._joinType ) {
          array = new SyntaxExpression[ex2._expressions.Length + 1];
          array[0] = ex1;
          Array.Copy( ex2._expressions, 0, array, 1, ex2._expressions.Length );
        }
        else {
          array = new[] { ex1, (SyntaxExpression)ex2 };
        }

        return new SyntaxExpressionJoiner( array, joinType );
      }
      private static SyntaxExpressionJoiner Join( SyntaxExpressionJoiner ex1, SyntaxExpressionJoiner ex2, JoinType joinType )
      {
        if ( joinType != ex1._joinType ) {
          return Join( (SyntaxExpression)ex1, ex2, joinType );
        }
        if ( joinType != ex2._joinType ) {
          return Join( ex1, (SyntaxExpression)ex2, joinType );
        }

        var array = new SyntaxExpression[ex1._expressions.Length + ex2._expressions.Length];
        Array.Copy( ex1._expressions, array, ex1._expressions.Length );
        Array.Copy( ex2._expressions, 0, array, ex1._expressions.Length, ex2._expressions.Length );

        return new SyntaxExpressionJoiner( array, joinType );
      }

      public static implicit operator SyntaxExpression( SyntaxExpressionJoiner joiner )
      {
        switch ( joiner._joinType ) {
          case JoinType.Serial: return new SerialExpression( joiner._expressions );
          case JoinType.Parallel: return new ParallelExpression( joiner._expressions );

          default: throw new InvalidOperationException();
        }
      }
    }

    protected class Context
    {
      private readonly HashSet<ExpressionPositionPair> _executingExpressions = new HashSet<ExpressionPositionPair>();

      public SyntaxException SyntaxException { get; set; }

      public string Text { get; private set; }

      public int CurrentPosition { get; set; }

      public Context( string text )
      {
        Text = text;
        CurrentPosition = 0;
      }

      public IDisposable StartCompositeExpression( SyntaxExpression expression )
      {
        var expressionPosition = new ExpressionPositionPair( expression, CurrentPosition );

        if ( false == _executingExpressions.Add( expressionPosition ) ) return null;

        return new CompositeExpressionStarter( this, expressionPosition );
      }

      private class CompositeExpressionStarter : IDisposable
      {
        private readonly Context _context;
        private readonly ExpressionPositionPair _expressionPosition;

        public CompositeExpressionStarter( Context context, ExpressionPositionPair expressionPosition )
        {
          _context = context;
          _expressionPosition = expressionPosition;
        }

        public void Dispose()
        {
          _context._executingExpressions.Remove( _expressionPosition );
        }
      }
    }

    private class ErrorOnEmptyExpression : SyntaxExpression
    {
      private readonly string _errorString;

      public ErrorOnEmptyExpression( string errorString )
      {
        _errorString = errorString;
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression ) { }

      protected override SyntaxTreeNode Parse( Context context )
      {
        context.SyntaxException = SyntaxException.SyntaxError( this.Name, context.CurrentPosition, _errorString ) ;

        return SyntaxTreeNode.SimpleValue( this, context.Text, context.CurrentPosition, 0 );
      }
    }

    private class NameExpression : SyntaxExpression
    {
      private readonly Regex _regexp;
      private readonly HashSet<string> _names ;
      private readonly string _messageOnError ;

      public NameExpression( Regex pattern, IEnumerable<string> allowedNames, string messageOnError )
      {
        _regexp = pattern ;
        _messageOnError = messageOnError ?? "Invalid name: \"{0}\"" ;

        if ( null != allowedNames ) {
          _names = new HashSet<string>( allowedNames.Select( x => x.ToLowerInvariant() ) ) ;
        }
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression ) { }

      protected override SyntaxTreeNode Parse( Context context )
      {
        if ( null == _regexp ) return null;

        var startIndex = context.CurrentPosition;

        var match = _regexp.Match( context.Text, startIndex );
        if ( !match.Success || match.Index != startIndex ) {
          context.SyntaxException = SyntaxException.RequiresPattern( this.Name, startIndex, _regexp );
          return null;
        }

        context.CurrentPosition += match.Length;

        if ( null != _names && ! _names.Contains( match.Value.ToLowerInvariant() ) ) {
          context.SyntaxException = SyntaxException.SyntaxError( this.Name, startIndex, string.Format( _messageOnError, match.Value ) ) ;
        }
        else {
          context.SyntaxException = null ;
        }

        return SyntaxTreeNode.SimpleValue( this, context.Text, startIndex, match.Length );
      }
    }

    private class NormalExpression : SyntaxExpression
    {
      private readonly string _text;

      public NormalExpression( string text )
      {
        _text = text;
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression ) { }

      protected override SyntaxTreeNode Parse( Context context )
      {
        var startIndex = context.CurrentPosition;

        int n = _text.Length;
        if ( 0 != string.Compare( _text, 0, context.Text, context.CurrentPosition, n ) ) {
          context.SyntaxException = SyntaxException.RequiresString( this.Name, startIndex, _text );
          return null;
        }

        context.CurrentPosition += n;

        context.SyntaxException = null ;

        return SyntaxTreeNode.SimpleValue( this, context.Text, startIndex, n );
      }
    }

    private class RegexExpression : SyntaxExpression
    {
      private readonly Regex _regexp;

      public RegexExpression( string pattern, RegexOptions options )
      {
        if ( string.IsNullOrEmpty( pattern ) ) {
          _regexp = null;
        }
        else {
          _regexp = new Regex( pattern, options | RegexOptions.Singleline | RegexOptions.CultureInvariant );
        }
      }

      public RegexExpression( Regex regex )
      {
        _regexp = regex ;
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression ) { }

      protected override SyntaxTreeNode Parse( Context context )
      {
        if ( null == _regexp ) return null;

        var startIndex = context.CurrentPosition;

        var match = _regexp.Match( context.Text, startIndex );
        if ( !match.Success || match.Index != startIndex ) {
          context.SyntaxException = SyntaxException.RequiresPattern( this.Name, startIndex, _regexp );
          return null;
        }

        context.CurrentPosition += match.Length;

        context.SyntaxException = null ;

        return SyntaxTreeNode.SimpleValue( this, context.Text, startIndex, match.Length );
      }
    }

    private class SerialExpression : SyntaxExpression
    {
      private readonly SyntaxExpression[] _expressions;

      public SerialExpression( SyntaxExpression[] expressions )
      {
        _expressions = expressions.Clone() as SyntaxExpression[];
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression )
      {
        for ( int i = 0 ; i < _expressions.Length ; ++i ) {
          if ( _expressions[i].PlaceholderName == name ) {
            _expressions[i] = expression;
          }
          else {
            _expressions[i].ReplacePlaceholder( name, expression );
          }
        }
      }

      protected override SyntaxTreeNode Parse( Context context )
      {
        using ( var expression = context.StartCompositeExpression( this ) ) {
          if ( null == expression ) {
            context.SyntaxException = SyntaxException.InfiniteDefinitionLoop( this.Name, context.CurrentPosition );
            return null;
          }

          var startIndex = context.CurrentPosition;

          SyntaxException foundSyntaxError = null ;
          var trees = new List<SyntaxTreeNode>( _expressions.Length );
          foreach ( var ex in _expressions ) {
            var tree = ex.Parse( context );
            if ( null == tree ) {
              context.CurrentPosition = startIndex;
              return null;
            }

            if ( null == foundSyntaxError && null != context.SyntaxException ) {
              foundSyntaxError = context.SyntaxException ;
              context.SyntaxException = null ;
            }

            trees.Add( tree );
          }

          context.SyntaxException = foundSyntaxError ;

          return SyntaxTreeNode.Serial( this, context.Text, startIndex, trees.ToArray() );
        }
      }
    }

    private class ParallelExpression : SyntaxExpression
    {
      private readonly SyntaxExpression[] _expressions;

      public ParallelExpression( SyntaxExpression[] expressions )
      {
        _expressions = expressions.Clone() as SyntaxExpression[];
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression )
      {
        for ( int i = 0 ; i < _expressions.Length ; ++i ) {
          if ( _expressions[i].PlaceholderName == name ) {
            _expressions[i] = expression;
          }
          else {
            _expressions[i].ReplacePlaceholder( name, expression );
          }
        }
      }

      protected override SyntaxTreeNode Parse( Context context )
      {
        using ( var expression = context.StartCompositeExpression( this ) ) {
          if ( null == expression ) {
            context.SyntaxException = SyntaxException.InfiniteDefinitionLoop( this.Name, context.CurrentPosition );
            return null;
          }

          var startIndex = context.CurrentPosition;

          SyntaxException validButError = null ;
          SyntaxTreeNode validButErrorNode = null ;
          int validButErrorEndPosition = -1 ;
          
          var exceptions = new List<SyntaxException>( _expressions.Length );
          foreach ( var ex in _expressions ) {
            var tree = ex.Parse( context );
            if ( null != tree ) {
              if ( null != this.Name ) {
                tree = SyntaxTreeNode.Wrap( this.Name, tree );
              }

              if ( null != context.SyntaxException ) {
                if ( null == validButError ) {
                  validButError = context.SyntaxException ;
                  validButErrorNode = tree ;
                  validButErrorEndPosition = context.CurrentPosition ;
                }

                context.SyntaxException = null ;
                context.CurrentPosition = startIndex;
                continue ;
              }
              
              return tree;
            }

            if ( null != context.SyntaxException ) {
              exceptions.Add( context.SyntaxException );
              context.SyntaxException = null ;
            }
            context.CurrentPosition = startIndex;
          }

          if ( null != validButErrorNode ) {
            context.SyntaxException = validButError ;
            context.CurrentPosition = validButErrorEndPosition ;
            return validButErrorNode ;
          }

          context.SyntaxException = SyntaxException.RequiresAny( this.Name, startIndex, exceptions.ToArray() );

          return null;
        }
      }
    }

    private class RepeatExpression : SyntaxExpression
    {
      private SyntaxExpression _exp;
      private readonly int _minCount;
      private readonly int _maxCount;
      private SyntaxExpression _delimiter;

      public RepeatExpression( SyntaxExpression exp, int minCount, int maxCount, SyntaxExpression delimiter )
      {
        _exp = exp;
        _minCount = minCount;
        _maxCount = maxCount;
        _delimiter = delimiter;
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression )
      {
        if ( _exp.PlaceholderName == name ) {
          _exp = expression;
        }
        else {
          _exp.ReplacePlaceholder( name, expression );
        }

        if ( null != _delimiter ) {
          if ( _delimiter.PlaceholderName == name ) {
            _delimiter = expression;
          }
          else {
            _delimiter.ReplacePlaceholder( name, expression );
          }
        }
      }

      protected override SyntaxTreeNode Parse( Context context )
      {
        using ( var expression = context.StartCompositeExpression( this ) ) {
          if ( null == expression ) {
            context.SyntaxException = SyntaxException.InfiniteDefinitionLoop( this.Name, context.CurrentPosition );
            return null;
          }

          int startIndex = context.CurrentPosition, lastDelimStart = startIndex;
          var list = new List<SyntaxTreeNode>();
          var count = 0;
          SyntaxException foundSyntaxError = null ;
          SyntaxTreeNode lastDelim = null;
          while ( count < _maxCount ) {
            var tree = _exp.Parse( context );
            if ( null == tree ) {
              // 要素終了
              if ( null != _delimiter && null != lastDelim ) {
                // デリミターがある場合、パースエラーを伝播
                return null;
              }
              break;
            }
            
            if ( null == foundSyntaxError && null != context.SyntaxException ) {
              foundSyntaxError = context.SyntaxException ;
              context.SyntaxException = null ;
            }

            if ( null != lastDelim ) {
              list.Add( lastDelim );
            }
            list.Add( tree );
            ++count;

            if ( null != _delimiter ) {
              lastDelimStart = context.CurrentPosition;
              lastDelim = _delimiter.Parse( context );
              if ( null == lastDelim ) {
                // デリミターが足りない
                break;
              }
            }
          }

          context.SyntaxException = foundSyntaxError ;

          if ( list.Count < _minCount ) {
            // 繰り返し数が足りない
            context.CurrentPosition = startIndex;
            if ( null == context.SyntaxException ) {
              context.SyntaxException = SyntaxException.RequiresRepeat( this.Name, context.SyntaxException, _minCount, list.Count );
            }
            return null;
          }
          else {
            // 繰り返し数が十分
            return SyntaxTreeNode.Serial( this, context.Text, startIndex, list.ToArray() );
          }
        }
      }
    }

    private class PlaceholderExpression : SyntaxExpression
    {
      private readonly string _placeholderName;

      public override string PlaceholderName { get { return _placeholderName; } }

      public PlaceholderExpression( string placeholderName )
      {
        _placeholderName = placeholderName;
      }

      public override void ReplacePlaceholder( string name, SyntaxExpression expression )
      {
        throw new InvalidOperationException();
      }

      protected override SyntaxTreeNode Parse( Context context )
      {
        throw new InvalidOperationException();
      }
    }
  }
}
