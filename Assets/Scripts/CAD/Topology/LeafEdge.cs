using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System;
using System.Linq;
using MaterialUI ;

namespace Chiyoda.CAD.Topology
{
  [Flags]
  public enum PositionMode
  {
    Variable = 0x00,
    FixedX = 0x01,
    FixedY = 0x02,
    FixedZ = 0x04,
    FixedXY = FixedX | FixedY,
    FixedYZ = FixedY | FixedZ,
    FixedZX = FixedZ | FixedX,
    FixedAll = FixedX | FixedY | FixedZ,
    Fixed = FixedAll,
  }

  [System.Serializable]
  [Entity( EntityType.Type.LeafEdge )]
  public class LeafEdge : Edge
  {
    private readonly MementoDictionary<int, HalfVertex> _vertices;
    private readonly Memento<Line> _line;

    private readonly Memento<PositionMode> _positionMode;
    private readonly Memento<PipingPiece> _pipingPiece;
    private Bounds _lastPipingPieceBound = new Bounds();

    private readonly INamedProperty[] _positionProperties;

    public event EventHandler<ValueChangedEventArgs<Line>> LineChanging;
    public event EventHandler<ValueChangedEventArgs<Line>> LineChanged;

    public LeafEdge( Document document ) : base( document )
    {
      _vertices = CreateMementoDictionaryAndSetupChildrenEvents<int, HalfVertex>() ;

      _line = new Memento<Line>( this );
 
      _positionMode = new Memento<PositionMode>( this, PositionMode.Variable );
      _pipingPiece = CreateMementoAndSetupChildrenEvents<PipingPiece>();

      _positionProperties = new[] {
        RegisterDelayedProperty( "PosX", PropertyType.Length, GetPosX, SetPosX, null ),
        RegisterDelayedProperty( "MinX", PropertyType.Length, GetMinX, SetMinX, null ),
        RegisterDelayedProperty( "MaxX", PropertyType.Length, GetMaxX, SetMaxX, null ),
        RegisterDelayedProperty( "PosY", PropertyType.Length, GetPosY, SetPosY, null ),
        RegisterDelayedProperty( "MinY", PropertyType.Length, GetMinY, SetMinY, null ),
        RegisterDelayedProperty( "MaxY", PropertyType.Length, GetMaxY, SetMaxY, null ),
        RegisterDelayedProperty( "PosZ", PropertyType.Length, GetPosZ, SetPosZ, null ),
        RegisterDelayedProperty( "MinZ", PropertyType.Length, GetMinZ, SetMinZ, null ),
        RegisterDelayedProperty( "MaxZ", PropertyType.Length, GetMaxZ, SetMaxZ, null ),
        RegisterDelayedProperty( "GlobalMinZ", PropertyType.Length, GetGlobalMinZ, null, null ),
        RegisterDelayedProperty( "GlobalMaxZ", PropertyType.Length, GetGlobalMaxZ, null, null ),
      };
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects() ;
      
      _line.AfterValueChanged += ( sender, e ) => OnLineChanged( e );
      _pipingPiece.AfterValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          e.OldValue.AfterNewlyValueChanged -= PipingPiece_AfterNewlyValueChanged;
          e.OldValue.AfterHistoricallyValueChanged -= PipingPiece_AfterHistoricallyValueChanged;
        }
        if ( null != e.NewValue ) {
          e.NewValue.AfterNewlyValueChanged += PipingPiece_AfterNewlyValueChanged;
          e.NewValue.AfterHistoricallyValueChanged += PipingPiece_AfterHistoricallyValueChanged;
        }
      };
      _pipingPiece.AfterNewlyValueChanged += ( sender, e ) => PipingPiece_AfterNewlyValueChanged( null, EventArgs.Empty ) ;
      _pipingPiece.AfterHistoricallyValueChanged += ( sender, e ) => PipingPiece_AfterHistoricallyValueChanged( null, EventArgs.Empty ) ;

      this.NewlyLocalCodChanged += ( sender, e ) =>
      {
        RefreshPositionCache();
      };
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as LeafEdge;
      _vertices.CopyFrom( entity._vertices.Select( pair => new KeyValuePair<int, HalfVertex>( pair.Key, pair.Value.GetCopyObjectOrClone( storage ) ) ) ) ;
      _line.CopyFrom( entity._line.Value.GetCopyObject( storage ) ) ;
      _positionMode.CopyFrom( entity._positionMode.Value );
      _pipingPiece.CopyFrom( entity._pipingPiece.Value.Clone( storage ) ) ;
      if ( null != _pipingPiece.Value ) {
        _pipingPiece.Value.AfterNewlyValueChanged += PipingPiece_AfterNewlyValueChanged;
        _pipingPiece.Value.AfterHistoricallyValueChanged += PipingPiece_AfterHistoricallyValueChanged;
      }
    }

    private bool IsConnectPointPositionChanged()
    {
      return Vertices.Any( v => ( null != v.Partner ) && Tolerance.DistanceTolerance * Tolerance.DistanceTolerance < Vector3d.SqrMagnitude( v.GlobalPoint - v.Partner.GlobalPoint ) ) ;
    }

    private void PipingPiece_AfterNewlyValueChanged( object sender, EventArgs e )
    {
      var ppBound = GetPipingPieceBounds( _pipingPiece.Value );
      if ( _lastPipingPieceBound != ppBound ) {
        _lastPipingPieceBound = ppBound;
        RefreshPositionCache();
        Document.RegisterEdgeMoved( this );

        if ( IsConnectPointPositionChanged() ) {
          Document.RegisterConnectionMaintenance( ConnectionMaintenanceOrigin );
        }
      }
    }

    private void PipingPiece_AfterHistoricallyValueChanged( object sender, EventArgs e )
    {
      var ppBound = GetPipingPieceBounds( _pipingPiece.Value );
      if ( _lastPipingPieceBound != ppBound ) {
        _lastPipingPieceBound = ppBound ;
        RefreshPositionCache() ;
      }
    }

    private static Bounds GetPipingPieceBounds( PipingPiece pp )
    {
      if ( null == pp ) return new Bounds();
      return pp.GetBounds();
    }

    public Line Line
    {
      get => _line.Value;
      set
      {
        if ( null != _line.Value ) {
          _line.Value.RemoveLeafEdge( this );
        }

        _line.Value = value;

        if ( null != _line.Value ) {
          _line.Value.AddLeafEdge( this );
        }
      }
    }

    protected virtual void OnLineChanged( ValueChangedEventArgs<Line> e )
    {
      LineChanged?.Invoke( this, e );
    }

    public override IEnumerable<HalfVertex> Vertices
    {
      get { return _vertices.Values; }
    }

    public override int VertexCount => _vertices.Count ;

    public HalfVertex GetVertex( int connectPointNumber )
    {
      if ( ! _vertices.TryGetValue( connectPointNumber, out var halfVertex ) ) return null ;
      return halfVertex ;
    }

    internal void SwapVertex( HalfVertex oldVertex, HalfVertex newVertex )
    {
      if ( oldVertex == newVertex ) return ;
      
      var index1 = oldVertex.ConnectPointIndex ;
      var index2 = newVertex.ConnectPointIndex ;

      newVertex.ConnectPointIndex = index1 ;
      oldVertex.ConnectPointIndex = index2 ;

      var anotherLeafEdge = newVertex.LeafEdge ;
      if ( null == anotherLeafEdge ) {
        _vertices[ index1 ] = newVertex ;
      }
      else {
        anotherLeafEdge._vertices.Remove( index2 ) ;  // 一旦外さないと、「親から外していない」エラーになる
        _vertices[ index1 ] = newVertex ;
        anotherLeafEdge._vertices.Add( index2, oldVertex ) ;
      }
    }

    public PositionMode PositionMode
    {
      get => _positionMode.Value ;
      set => _positionMode.Value = value ;
    }

    public PipingPiece PipingPiece
    {
      get => _pipingPiece.Value ;
      set => _pipingPiece.Value = value ;
    }

    public override IEnumerable<Model.Component> GetAllComponents()
    {
      if (_pipingPiece.Value is Model.Component comp) {
        yield return comp;
      }
    }

    public override Bounds? GetGlobalBounds()
    {
      return PipingPiece?.GetGlobalBounds();
    }


    public override IEnumerable<IElement> Children
    {
      get
      {
        if ( null != _pipingPiece.Value ) {
          yield return _pipingPiece.Value;
        }

        foreach ( var v in _vertices.Values ) {
          yield return v ;
        }
      }
    }
    
    #region 組み込みプロパティ

    private Box3d? _boxCache; // null設定の高速化のため、NullではなくNullableのnullを用いる

    public void ForceTriggerPositionChange()
    {
      RefreshPositionCache();

      var rulingProps = new List<INamedProperty>( _positionProperties.Length );
      foreach ( var prop in _positionProperties ) {
        if ( false == prop.IsDelaying ) {
          rulingProps.Add( prop );
        }
      }
      foreach ( var prop in rulingProps ) {
        Document.RegisterPropertyChange( prop );
      }
    }

    private void RefreshPositionCache()
    {
      _boxCache = null;
    }

    private void TryRefreshPositionCache()
    {
      if ( _boxCache.HasValue ) return;

      if ( null == PipingPiece ) {
        _boxCache = new Box3d( LocalCod.Origin );
        return;
      }

      try {
        var bounds = LocalCod.GlobalizeBounds( PipingPiece.GetBounds() );
        _boxCache = new Box3d( bounds.min, bounds.max );
      }
      catch ( NotImplementedException ) {
        _boxCache = new Box3d( LocalCod.Origin );
      }
    }

    private double GetPosX()
    {
      return LocalCod.Origin.x;
    }
    private double GetMinX()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Min.x;
    }
    private double GetMaxX()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Max.x;
    }
    private void SetPosX( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.x = value;
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMinX( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.x += value - GetMinX();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMaxX( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.x += value - GetMaxX();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }

    private double GetPosY()
    {
      return LocalCod.Origin.y;
    }
    private double GetMinY()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Min.y;
    }
    private double GetMaxY()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Max.y;
    }
    private void SetPosY( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.y = value;
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMinY( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.y += value - GetMinY();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMaxY( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.y += value - GetMaxY();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }

    private double GetPosZ()
    {
      return LocalCod.Origin.z;
    }
    private double GetMinZ()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Min.z;
    }
    private double GetMaxZ()
    {
      TryRefreshPositionCache();
      return _boxCache.Value.Max.z;
    }
    private void SetPosZ( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.z = value;
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMinZ( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.z += value - GetMinZ();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }
    private void SetMaxZ( double value )
    {
      RefreshPositionCache();
      var pos = LocalCod.Origin;
      pos.z += value - GetMaxZ();
      LocalCod = new LocalCodSys3d( pos, LocalCod );
    }

    private double GetGlobalMinZ()
    {
      RefreshPositionCache();
      var minP = new Vector3d( GetMinX(), GetMinY(), GetMinZ() ) ;
      return GlobalCod.GlobalizePoint( minP ).z ;
    }
    
    private double GetGlobalMaxZ()
    {
      RefreshPositionCache();
      var maxP = new Vector3d( GetMaxX(), GetMaxY(), GetMaxZ() ) ;
      return GlobalCod.GlobalizePoint( maxP ).z ;
    }

    #endregion

    public override string ToString()
    {
      if ( null == PipingPiece ) {
        return base.ToString() + " (PipingPiece: null)";
      }
      else {
        return $"{base.ToString()} (PipingPiece: {PipingPiece.ToString()})";
      }
    }

    public void CreateAllHalfVertices()
    {
      if ( null == PipingPiece ) throw new InvalidOperationException( "PipingPiece is null." ) ;
      if ( 0 != _vertices.Count ) throw new InvalidOperationException( "Vertices are not empty." ) ;

      _vertices.SetRange( PipingPiece.ConnectPoints.Select( cp => new KeyValuePair<int, HalfVertex>( cp.ConnectPointNumber, CreateHalfVertex( cp.ConnectPointNumber ) ) ) ) ;
    }

    public HalfVertex AddVertex( int connectPointIndex )
    {
      var v = CreateHalfVertex( connectPointIndex ) ;
      _vertices.Add( connectPointIndex, v ) ;
      return v ;
    }

    private HalfVertex CreateHalfVertex( int connectPointIndex )
    {
      var vertex = Document.CreateEntity<HalfVertex>() ;
      vertex.ConnectPointIndex = connectPointIndex ;
      return vertex ;
    }

    public IEnumerable<HalfVertex> GetFreeVertex()
    {
      return Vertices.Where( vertex => vertex.Partner == null ) ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      if ( PipingPiece is Equipment ) {
        MirrorOriginOnly( origin, normalDirection ) ;
      }
      else {
        MirrorXY( origin, normalDirection ) ;
      }
    }

    private void MirrorOriginOnly( in Vector3d origin, in Vector3d normalDirection )
    {
      LocalCod = new LocalCodSys3d( LocalCod.Origin.MirrorPointBy( origin, normalDirection ), LocalCod ) ;
    }

    private void MirrorXY( in Vector3d origin, in Vector3d normalDirection )
    {
      if ( normalDirection.IsParallelTo( Vector3d.forward ) ) {
        LocalCod = LocalCod.MirrorXYBy( origin, normalDirection ) ;
      }
      else {
        var orgDeg = ExtraHorizontalRotationDegree ;
        if ( Math.Abs( orgDeg ) < Tolerance.AngleTolerance ) {
          LocalCod = LocalCod.MirrorXYBy( origin, normalDirection ) ;
        }
        else {
          if ( false == normalDirection.IsPerpendicularTo( Vector3d.forward ) ) {
            // 垂直でも水平でもない場合は ExtraHorizontalRotationDegree の値がおかしくなるため、警告
            Debug.LogWarning( "Mirrored by bad plane!" ) ;
          }

          ExtraHorizontalRotationDegree = 0 ;
          LocalCod = LocalCod.MirrorXYBy( origin, normalDirection ) ;
          ExtraHorizontalRotationDegree = -orgDeg ;
        }
      }
    }

    protected internal override Edge DisassembleImpl()
    {
      UnbindAllRules() ;

      return this ;
    }

    internal override void SetMinimumLengthRatioByDiameterForAllPipes( double newRatio )
    {
      if ( PipingPiece is Pipe pipe ) {
        pipe.MinimumLengthRatioByDiameter = newRatio ;
      }
    }
  }
}