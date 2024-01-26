using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.SyntaxTree
{
  public abstract class SyntaxTreeNode
  {
    public string ExpressionName { get; }

    public static SyntaxTreeNode SimpleValue( SyntaxExpression expression, string text, int startIndex, int length )
    {
      return new SyntaxLeafNode( expression.Name, text, startIndex, length );
    }

    public static SyntaxTreeNode Serial( SyntaxExpression expression, string text, int startIndex, params SyntaxTreeNode[] trees )
    {
      return new SyntaxSerialNode( expression.Name, text, startIndex, trees );
    }

    public static SyntaxTreeNode Wrap( string name, SyntaxTreeNode tree )
    {
      return new SyntaxSerialNode( name, tree.Text, tree.StartIndex, new[] { tree } );
    }



    public abstract IEnumerable<SyntaxTreeNode> GetNamedChildren();

    public string Text { get; }

    public string Value { get { return Text.Substring( StartIndex, EndIndex - StartIndex ); } }

    public int StartIndex { get; }

    public int EndIndex { get; }

    protected SyntaxTreeNode( string expressionName, string text, int startIndex, int endIndex )
    {
      Text = text;
      StartIndex = startIndex;
      EndIndex = endIndex;
      ExpressionName = expressionName;
    }

    public override string ToString()
    {
      return $"{( ExpressionName ?? "(none)" )} ({StartIndex}-{EndIndex}): {Value}" ;
    }


    private class SyntaxLeafNode : SyntaxTreeNode
    {
      public override IEnumerable<SyntaxTreeNode> GetNamedChildren()
      {
        yield break;
      }

      public SyntaxLeafNode( string expressionName, string text, int startIndex, int length )
        : base( expressionName, text, startIndex, startIndex + length )
      { }
    }

    private class SyntaxSerialNode : SyntaxTreeNode
    {
      private readonly SyntaxTreeNode[] _series;

      public override IEnumerable<SyntaxTreeNode> GetNamedChildren()
      {
        foreach ( var node in _series ) {
          if ( null != node.ExpressionName ) {
            yield return node;
          }
          else {
            foreach ( var subnode in node.GetNamedChildren() ) {
              yield return subnode;
            }
          }
        }
      }

      public SyntaxSerialNode( string expressionName, string text, int startIndex, SyntaxTreeNode[] series )
        : base( expressionName, text, startIndex, GetMaxEndIndex( startIndex, series ) )
      {
        _series = series;
      }

      private static int GetMaxEndIndex( int startIndex, SyntaxTreeNode[] series )
      {
        if ( 0 == series.Length ) {
          return startIndex;
        }
        else {
          return series[series.Length - 1].EndIndex;
        }
      }
    }
  }
}
