using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System.Linq;
using System;

namespace Chiyoda.CAD.Topology
{
  [System.Serializable]
  public abstract class CompositeEdge : Edge, IGroup
  {
    protected readonly MementoList<Edge> _edgeList;
    private HashSet<HalfVertex> _endVertices = null;

    protected CompositeEdge( Document document ) : base( document )
    {
      _edgeList = CreateMementoListAndSetupChildrenEvents<Edge>() ;
      _edgeList.AfterItemChanged += ( sender, e ) =>
      {
        RefreshEndVerticesCache();
      };
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as CompositeEdge;
      _edgeList.AddRange( entity._edgeList.Select( src => src.GetCopyObjectOrClone( storage ) ) );
      RefreshEndVerticesCache();
    }

    public IEnumerable<Edge> EdgeList
    {
      get { return _edgeList; }
    }

    public int EdgeCount { get { return _edgeList.Count; } }

    public virtual bool AddEdge( Edge edge )
    {
      _edgeList.Add( edge );
      return true;
    }

    public virtual bool RemoveEdge( Edge edge )
    {
      return _edgeList.Remove( edge );
    }

    public virtual bool ReplaceEdge( int index, Edge newEdge )
    {
      if ( index < 0 || _edgeList.Count <= index ) return false ;

      _edgeList[ index ] = newEdge ;

      return true ;
    }

    public bool ReplaceEdge( Edge oldEdge, Edge newEdge )
    {
      int index = _edgeList.IndexOf( oldEdge ) ;
      if ( index < 0 ) return false ;

      return ReplaceEdge( index, newEdge ) ;
    }

    public override IEnumerable<HalfVertex> Vertices
    {
      get
      {
        return GetVertices();
      }
    }

    protected HashSet<HalfVertex> GetVertices()
    {
      if ( null == _endVertices ) _endVertices = CollectAllEndVertices();
      return _endVertices;
    }

    public override int VertexCount { get { return GetVertices().Count; } }

    private void RefreshEndVerticesCache()
    {
      _endVertices = null;
    }

    private HashSet<HalfVertex> CollectAllEndVertices()
    {
      var hashset = new HashSet<HalfVertex>();
      foreach ( var edge in EdgeList ) {
        foreach ( var v in edge.Vertices ) {
          if ( null == v.Partner ) {
            hashset.Add( v ) ;
          }
          else if ( ! this.IsAncestorOf( v.Partner.LeafEdge ) ) {
            hashset.Add( v ) ;
          }
        }
      }

      return hashset;
    }

    public IEnumerable<LeafEdge> GetAllLeafEdges()
    {
      foreach ( var edge in EdgeList ) {
        if ( edge is LeafEdge le ) yield return le;
        else if (edge is CompositeEdge ce ) {
          foreach ( var e in ce.GetAllLeafEdges() ) yield return e;
        }
      }
    }

    public override IEnumerable<Model.Component> GetAllComponents()
    {
      foreach ( var edge in EdgeList ) {
        foreach ( var c in edge.GetAllComponents() ) {
          yield return c;
        }
      }
    }

    public override Bounds? GetGlobalBounds()
    {
      return CAD.Boundary.GetBounds( _edgeList );
    }

    public override IEnumerable<IElement> Children => EdgeList;
    
    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      var planeCodSys = new LocalCodSys3d( origin, Vector3d.zero, Vector3d.zero, normalDirection ) ;
      
      if ( normalDirection.IsParallelTo( Vector3d.forward ) ) {
        LocalCod = CreateMirrorLocalCod( origin, normalDirection ) ;
      }
      else {
        var orgDeg = ExtraHorizontalRotationDegree ;
        if ( Math.Abs( orgDeg ) < Tolerance.AngleTolerance ) {
          LocalCod = CreateMirrorLocalCod( origin, normalDirection ) ;
        }
        else {
          if ( false == normalDirection.IsPerpendicularTo( Vector3d.forward ) ) {
            // 垂直でも水平でもない場合は ExtraHorizontalRotationDegree の値がおかしくなるため、警告
            Debug.LogWarning( "Mirrored by bad plane!" ) ;
          }

          ExtraHorizontalRotationDegree = 0 ;
          LocalCod = CreateMirrorLocalCod( origin, normalDirection ) ;
          ExtraHorizontalRotationDegree = -orgDeg ;
        }
      }

      var newCodSys = LocalCod.LocalizeCodSys( planeCodSys ) ;
      var o = newCodSys.Origin ;
      var dir = newCodSys.DirectionZ ;

      foreach ( var edge in EdgeList ) {
        edge.Mirror( o, dir ) ;
      }
    }
    private LocalCodSys3d CreateMirrorLocalCod( in Vector3d origin, in Vector3d normalDirection )
    {
      return new LocalCodSys3d( LocalCod.Origin.MirrorPointBy( origin, normalDirection ), LocalCod ) ;
    }

    protected internal override Edge DisassembleImpl()
    {
      UnbindAllRules() ;

      // 子孫を全て分解する
      for ( int i = 0, n = _edgeList.Count ; i < n ; ++i ) {
        ReplaceEdge( i, _edgeList[ i ].DisassembleImpl() ) ;
      }

      return this ;
    }

    internal override void SetMinimumLengthRatioByDiameterForAllPipes( double newRatio )
    {
      foreach ( var edge in EdgeList ) {
        edge.SetMinimumLengthRatioByDiameterForAllPipes( newRatio );
      }
    }
  }
}