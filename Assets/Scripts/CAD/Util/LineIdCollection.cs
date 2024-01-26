using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Util
{
  public abstract class EdgeObserver
  {
    public bool IsStarted { get; private set; }
    public Edge ObserveTarget { get; }

    public EdgeObserver( Edge elm, bool start )
    {
      ObserveTarget = elm;
      if ( start ) {
        Start();
      }
    }

    protected abstract void Register( Edge edge );

    protected abstract void Unregister( Edge edge );

    public void Start()
    {
      if ( !IsStarted ) {
        IsStarted = true;
        ObserveRecursive( ObserveTarget );
      }
    }
    public void Stop()
    {
      if ( IsStarted ) {
        IsStarted = false;
        UnobserveRecursive( ObserveTarget );
      }
    }

    private void ObserveRecursive( Edge elm )
    {
      Register( elm );
      foreach ( var child in elm.Children.OfType<Edge>() ) {
        ObserveRecursive( child );
      }
      elm.AfterNewlyChildrenChanged += Elm_ChildrenChanged;
    }

    private void UnobserveRecursive( Edge elm )
    {
      Unregister( elm );
      foreach ( var child in elm.Children.OfType<Edge>() ) {
        UnobserveRecursive( child );
      }
      elm.AfterNewlyChildrenChanged -= Elm_ChildrenChanged;
    }

    private void Elm_ChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      foreach ( var item in e.RemovedItems.OfType<Edge>() ) UnobserveRecursive( item );
      foreach ( var item in e.AddedItems.OfType<Edge>() ) ObserveRecursive( item );
    }
  }

  public class LineIdObserver : EdgeObserver
  {
    private readonly Dictionary<Line, int> _lineCount = new Dictionary<Line, int>();

    public LineIdObserver( Edge edge )
      : base( edge, true )
    { }

    public IEnumerable<Line> Lines => _lineCount.Keys;

    public int LineCount => _lineCount.Count;

    protected override void Register( Edge edge )
    {
      if ( edge is LeafEdge le ) {
        le.LineChanged += LeafEdge_LineChanged;
        AddLine( le.Line );
      }
    }

    protected override void Unregister( Edge edge )
    {
      if ( edge is LeafEdge le ) {
        le.LineChanged -= LeafEdge_LineChanged;
        RemoveLine( le.Line );
      }
    }

    private void LeafEdge_LineChanged( object sender, ValueChangedEventArgs<Line> e )
    {
      RemoveLine( e.OldValue );
      AddLine( e.NewValue );
    }

    private void AddLine( Line line )
    {
      if ( null == line ) return;

      if ( _lineCount.TryGetValue( line, out int value ) ) {
        _lineCount[line] = value + 1;
      }
      else {
        _lineCount.Add( line, 1 );
      }
    }

    private void RemoveLine( Line line )
    {
      if ( null == line ) return;

      if ( _lineCount.TryGetValue( line, out int value ) ) {
        if ( 1 == value ) {
          _lineCount.Remove( line );
        }
        else {
          _lineCount[line] = value - 1;
        }
      }
      else {
        throw new InvalidOperationException();
      }
    }
  }
}
