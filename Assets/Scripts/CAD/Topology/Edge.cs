using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  [System.Serializable]
  public abstract class Edge : Topology, IRelocatable
  {
    public event EventHandler NewlyLocalCodChanged;
    public event EventHandler HistoricallyLocalCodChanged;
    
    public event EventHandler LocalCodChanged
    {
      add
      {
        NewlyLocalCodChanged += value ;
        HistoricallyLocalCodChanged += value ;
      }
      remove
      {
        NewlyLocalCodChanged -= value ;
        HistoricallyLocalCodChanged -= value ;
      }
    }

    private readonly Memento<LocalCodSys3d> _localCod;
    private readonly Memento<double> _exRotation;
    private readonly Memento<LeafEdge> _connectionMaintenanceOrigin;

    protected Edge( Document document ) : base( document )
    {
      _localCod = CreateMemento( LocalCodSys3d.Identity ) ;
      _localCod.AfterNewlyValueChanged += OnNewlyLocalCodChanged ;
      _localCod.AfterHistoricallyValueChanged += OnHistoricallyLocalCodChanged ;

      _exRotation = CreateMemento( 0.0 ) ;
      _exRotation.AfterNewlyValueChanged += OnNewlyLocalCodChanged ;
      _exRotation.AfterHistoricallyValueChanged += OnHistoricallyLocalCodChanged ;

      _connectionMaintenanceOrigin = CreateMemento<LeafEdge>() ;

      RegisterDelayedProperty(
        "HorizontalRotationDegree",
        PropertyType.Angle,
        () => ExtraHorizontalRotationDegree,
        value => ExtraHorizontalRotationDegree = value,
        null );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as Edge ;
      _localCod.CopyFrom( entity._localCod.Value ) ;
      _exRotation.CopyFrom( entity._exRotation.Value ) ;
      storage.RegisterAfterAll( () => _connectionMaintenanceOrigin.CopyFrom( entity._connectionMaintenanceOrigin.Value?.GetCopyObject( storage ) ?? entity._connectionMaintenanceOrigin.Value ) ) ;
    }

    protected virtual void OnNewlyLocalCodChanged( object sender, EventArgs e )
    {
      Document.RegisterConnectionMaintenance( ConnectionMaintenanceOrigin ) ;
      Document.RegisterEdgeMoved( this ) ;

      NewlyLocalCodChanged?.Invoke( this, EventArgs.Empty ) ;
    }

    protected virtual void OnHistoricallyLocalCodChanged( object sender, EventArgs e )
    {
      HistoricallyLocalCodChanged?.Invoke( this, EventArgs.Empty ) ;
    }

    public LeafEdge ConnectionMaintenanceOrigin
    {
      get => _connectionMaintenanceOrigin.Value ;
      set => _connectionMaintenanceOrigin.Value = value ;
    }

    public abstract IEnumerable<HalfVertex> Vertices { get; }

    public abstract int VertexCount { get; }

    public abstract override IEnumerable<IElement> Children
    {
      get;
    }


    public IGroup Group => Parent as IGroup ;

    public abstract IEnumerable<Model.Component> GetAllComponents();

    public LocalCodSys3d ParentCod
    {
      get
      {
        if ( null == Group ) {
          return LocalCodSys3d.Identity;
        }
        else {
          return Group.GlobalCod;
        }
      }
    }

    public LocalCodSys3d GlobalCod
    {
      get
      {
        return ParentCod.GlobalizeCodSys( LocalCod );
      }
    }

    public LocalCodSys3d LocalCod
    {
      get
      {
        if ( 0 == _exRotation.Value ) {
          return _localCod.Value;
        }
        else {
          var cod = _localCod.Value;
          var q = cod.Rotation;
          var r1 = Quaternion.AngleAxis( (float)_exRotation.Value, new Vector3( 0, 0, 1 ) );
          return new LocalCodSys3d( cod.Origin, r1 * q, cod.IsMirrorType ) ;
        }
      }

      set
      {
        if ( 0 == _exRotation.Value ) {
          _localCod.Value = value;
        }
        else {
          var r1 = Quaternion.AngleAxis( -(float)_exRotation.Value, new Vector3( 0, 0, 1 ) );
          _localCod.Value = new LocalCodSys3d( value.Origin, r1 * value.Rotation, value.IsMirrorType );
        }
      }
    }

    public LocalCodSys3d GetCodSysInAncestor( Edge ancestorEdge )
    {
      if ( null == ancestorEdge ) return GlobalCod ;
      if ( this == ancestorEdge ) return LocalCodSys3d.Identity ;

      for ( var e = this ; e != ancestorEdge ; e = e.Parent as Edge ) {
        if ( null == e ) throw new InvalidOperationException($"{this} don't have ancestor {ancestorEdge}.") ;
      }

      var codsys = LocalCodSys3d.Identity ;
      for ( var e = this ; e != ancestorEdge ; e = e.Parent as Edge ) {
        codsys = e.LocalCod.GlobalizeCodSys( codsys ) ;
      }

      return codsys ;
    }

    public double ExtraHorizontalRotationDegree
    {
      get { return _exRotation.Value; }
      set { _exRotation.Value = value; }
    }

    public static bool IsContinuous( params Edge[] edges )
    {
      return IsContinuous( (IEnumerable<Edge>)edges );
    }
    public static bool IsContinuous( IEnumerable<Edge> edges )
    {
      var firstEdge = edges.FirstOrDefault();
      if ( null == firstEdge ) return false;

      // vertex→edge
      var v2e = new Dictionary<HalfVertex, (Edge, Edge)>();
      int edgeCount = 0;
      foreach ( var edge in edges ) {
        ++edgeCount;
        foreach ( var v in edge.Vertices ) {
          if ( ! v2e.TryGetValue( v.Partner ?? v, out var pair ) ) {
            v2e.Add( v, ( edge, null ) ) ;
          }
          else if ( null != pair.Item2 ) {
            return false ;
          }
          else {
            v2e[ v.Partner ?? v ] = ( pair.Item1, edge ) ;
          }
        }
      }

      // 最初のエッジから順に追ってゆく
      var doneEdges = new HashSet<Edge>();
      var vertexStack = new Stack<HalfVertex>();
      vertexStack.Push( firstEdge.Vertices.First() );

      while ( 0 != vertexStack.Count ) {
        var v = vertexStack.Pop();
        if ( ! v2e.TryGetValue( v, out var pair ) && ! v2e.TryGetValue( v, out pair ) ) {
          throw new InvalidOperationException() ;
        }

        if ( doneEdges.Add( pair.Item1 ) ) {
          foreach ( var v2 in pair.Item1.Vertices ) {
            if ( v2 != v && v2 != v.Partner ) vertexStack.Push( v2 ) ;
          }
        }
        if ( null != pair.Item2 && doneEdges.Add( pair.Item2 ) ) {
          foreach ( var v2 in pair.Item2.Vertices ) {
            if ( v2 != v && v2 != v.Partner ) vertexStack.Push( v2 ) ;
          }
        }
      }

      if ( doneEdges.Count < edgeCount ) {
        // 全て追えなかった
        return false;
      }
      else {
        // 全て追えた
        return true;
      }
    }

    /// <summary>
    /// エッジを完全に削除する
    /// </summary>
    public void Unlink()
    {
      foreach ( var v in Vertices ) {
        v.Partner = null ;
      }

      foreach ( var leafEdge in this.CollectAllEdges().OfType<LeafEdge>() ) {
        leafEdge.Line = null ;
      }

      ( Parent as IGroup )?.RemoveEdge( this ) ;
    }
    
    public void Translate( Vector3d worldVec )
    {
      if ( worldVec.sqrMagnitude < Vector3d.kEpsilon * Vector3d.kEpsilon ) return;

      var vec = ParentCod.LocalizeVector( worldVec );
      LocalCod = new LocalCodSys3d( LocalCod.Origin + vec, LocalCod );
    }


    public void Disassemble()
    {
      ( Parent as IGroup )?.ReplaceEdge( this, DisassembleImpl() ) ;
    }

    protected internal abstract Edge DisassembleImpl() ;

    public abstract void Mirror( in Vector3d origin, in Vector3d normalDirection ) ;

    internal abstract void SetMinimumLengthRatioByDiameterForAllPipes( double newRatio ) ;
  }
}