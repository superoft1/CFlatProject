using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System;
using System.Linq;

namespace Chiyoda.CAD.Topology
{
  [System.Serializable]
  [Entity( EntityType.Type.Line )]
  public class Line : Entity
  {
    private readonly Memento<string> _lineId;
    private readonly Memento<string> _serviceClass;
    private readonly MementoSet<LeafEdge> _leafEdges;

    public event EventHandler LineIdChanging;
    public event EventHandler LineIdChanged;

    public Line( Document document ) : base( document )
    {
      _lineId = new Memento<string>( this, "" );
      _lineId.BeforeNewlyValueChanged += ( sender, e ) => OnLineIdChanging( EventArgs.Empty );
      _lineId.AfterNewlyValueChanged += ( sender, e ) => OnLineIdChanged( EventArgs.Empty );

      _serviceClass = new Memento<string>( this, "" );
      _leafEdges = new MementoSet<LeafEdge>( this );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as Line;
      _lineId.CopyFrom( entity._lineId.Value );
      _serviceClass.CopyFrom( entity._serviceClass.Value );
      _leafEdges.SetCopyObjectFrom( entity._leafEdges, storage );
    }

    protected void OnLineIdChanging( EventArgs e )
    {
      LineIdChanging?.Invoke( this, e );
    }
    protected void OnLineIdChanged( EventArgs e )
    {
      LineIdChanged?.Invoke( this, e );
    }

    [UI.Property( UI.PropertyCategory.LineNumber, "ID", Visibility = UI.PropertyVisibility.Editable )]
    public string LineId
    {
      get { return _lineId.Value; }
      set
      {
        if ( _lineId.Value != value ) {
          if ( null != Document.FindLine( value ) ) throw new InvalidOperationException();
        }
        _lineId.Value = value;
      }
    }

    [UI.Property( UI.PropertyCategory.ServiceClass, "ID", Visibility = UI.PropertyVisibility.Editable )]
    public string ServiceClass
    {
      get { return _serviceClass.Value; }
      set { _serviceClass.Value = value; }
    }

    public IEnumerable<LeafEdge> LeafEdges
    {
      get { return _leafEdges; }
    }

    internal bool AddLeafEdge( LeafEdge leafEdge )
    {
      if ( null == leafEdge ) return false;

      return _leafEdges.Add( leafEdge );
    }
    internal bool RemoveLeafEdge( LeafEdge leafEdge )
    {
      if ( null == leafEdge ) return false;

      return _leafEdges.Remove( leafEdge );
    }

    public override Bounds? GetGlobalBounds()
    {
      return LeafEdges.Select( le => le.GetGlobalBounds() ).UnionBounds();
    }



    private class EqualityComparerByLineId : IEqualityComparer<Line>
    {
      public bool Equals( Line x, Line y )
      {
        if ( x == y ) return true;
        if ( x == null || y == null ) return false;

        return x.LineId == y.LineId;
      }

      public int GetHashCode( Line obj )
      {
        if ( null == obj ) return 0;
        return obj.LineId.GetHashCode();
      }
    }
  }
}