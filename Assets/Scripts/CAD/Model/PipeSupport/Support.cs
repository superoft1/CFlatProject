using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.CAD.Model
{
  public enum SupportType
  {
    None,
    PipeShoe,
    Trunnion,
    TType,
  }

  [Entity( EntityType.Type.Support )]
  public class Support : Entity
  {
    private bool _needRecreateSupportTargets = true ;
    private readonly Memento<bool> _enabled ;
    private readonly Memento<SupportType> _supportType ;
    private readonly HashSet<LeafEdge> _supportTargets = new HashSet<LeafEdge>() ;
    private readonly Memento<SupportPositionBase> _supportPosition ;
    private readonly Memento<Vector3d> _supportDirection ;

    public Support( Document document ) : base( document )
    {
      _enabled = CreateMemento( true ) ;
      _enabled.AfterNewlyValueChanged += ( sender, e ) =>
      {
        if ( SupportType.None != _supportType.Value ) RecreateSupportShape() ;
      } ;

      _supportType = CreateMemento( SupportType.None ) ;
      _supportType.AfterNewlyValueChanged += ( sender, e ) =>
      {
        if ( true == _enabled.Value ) RecreateSupportShape() ;
      } ;
      _supportType.AfterHistoricallyValueChanged += ( sender, e ) => OnAfterHistoricallyValueChanged() ;

      _supportPosition = new Memento<SupportPositionBase>( this ) ;
      _supportPosition.AfterValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          e.OldValue.NewlyPositionChanged -= SupportPosition_NewlyPositionChanged ;
          e.OldValue.HistoricallyPositionChanged -= SupportPosition_HistoricallyPositionChanged ;
        }

        if ( null != e.NewValue ) {
          e.NewValue.NewlyPositionChanged += SupportPosition_NewlyPositionChanged ;
          e.NewValue.HistoricallyPositionChanged += SupportPosition_HistoricallyPositionChanged ;
        }
      } ;

      _supportDirection = new Memento<Vector3d>( this, new Vector3d( 0, 0, -1 ) ) ;
      _supportDirection.AfterNewlyValueChanged += ( sender, e ) => RecreateSupportShape() ;
      _supportDirection.AfterHistoricallyValueChanged += ( sender, e ) => OnAfterHistoricallyValueChanged() ;

      AfterHistoricallyValueChanged += ( sender, e ) => RequestSupportShapeRecreation() ;

      RegisterDelayedProperty( "PosX", PropertyType.Length, GetPosX, SetPosX, null ) ;
      RegisterDelayedProperty( "PosY", PropertyType.Length, GetPosY, SetPosY, null ) ;
      RegisterDelayedProperty( "PosZ", PropertyType.Length, GetPosZ, SetPosZ, null ) ;

      RegisterDelayedProperty(
        "Enabled",
        PropertyType.Boolean,
        () => Enabled ? 1.0 : 0.0,
        value => Enabled = value.ToBoolean(),
        null ) ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;

      _supportPosition.Value = new AbsoluteSupportPosition( this ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as Support ;
      _enabled.CopyFrom( entity._enabled.Value ) ;
      _supportType.CopyFrom( entity._supportType.Value ) ;
      _supportPosition.CopyFrom( entity._supportPosition.Value ) ;
      _supportDirection.CopyFrom( entity._supportDirection.Value ) ;
    }

    public IEnumerable<LeafEdge> SupportTargets
    {
      get
      {
        if ( _needRecreateSupportTargets ) {
          _needRecreateSupportTargets = false ;
          SeekTargets() ;
        }

        return _supportTargets ;
      }
    }

    public bool Enabled
    {
      get => _enabled.Value ;
      set => _enabled.Value = value ;
    }

    public SupportType SupportType
    {
      get => _supportType.Value ;
      set => _supportType.Value = value ;
    }

    public SupportPositionBase SupportPosition
    {
      get { return _supportPosition.Value ; }
      set { _supportPosition.Value = value ; }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Position", IsEditableForBlockPatternChildren = false )]
    public Vector3d SupportOrigin
    {
      get { return _supportPosition.Value.Position ; }
      set { _supportPosition.Value.Position = value ; }
    }

    private void SetPosX( double value )
    {
      var vec = SupportOrigin ;
      vec.x = value ;
      SupportOrigin = vec ;
    }

    private void SetPosY( double value )
    {
      var vec = SupportOrigin ;
      vec.y = value ;
      SupportOrigin = vec ;
    }

    private void SetPosZ( double value )
    {
      var vec = SupportOrigin ;
      vec.z = value ;
      SupportOrigin = vec ;
    }

    private double GetPosX()
    {
      return SupportOrigin.x ;
    }

    private double GetPosY()
    {
      return SupportOrigin.y ;
    }

    private double GetPosZ()
    {
      return SupportOrigin.z ;
    }

    public Vector3d SupportDirection
    {
      get
      {
        // TODO: 下、横、上などノズルと同じような定義があればそれを使う
        return _supportDirection.Value ;
      }
      set { _supportDirection.Value = value ; }
    }

    private void SupportPosition_NewlyPositionChanged( object sender, EventArgs e )
    {
      RecreateSupportShape() ;
    }

    private void SupportPosition_HistoricallyPositionChanged( object sender, EventArgs e )
    {
      OnAfterHistoricallyValueChanged() ;
    }

    protected override void OnNewlyParentChanged( ValueChangedEventArgs<IElement> e )
    {
      base.OnNewlyParentChanged( e ) ;

      // TODO: いずれかのエッジが移動したら RecreateSupportShape() するイベントを追加
    }

    private bool RequestSupportShapeRecreation()
    {
      if ( false == _needRecreateSupportTargets ) {
        _needRecreateSupportTargets = true ;
        return true ;
      }
      return false ;
    }

    private void RecreateSupportShape()
    {
      if ( RequestSupportShapeRecreation() ) {
        OnAfterNewlyValueChanged() ;
      }
    }

    private void SeekTargets()
    {
      if ( ! Enabled ) return ;

      var positionTarget = _supportPosition.Value?.Target ;
      if ( ! NeedSeekTargets( SupportType ) ) {
        // 単独
        if ( _supportTargets.Count != 1 || _supportTargets.First() != positionTarget ) {
          _supportTargets.Clear() ;
          if ( null == positionTarget ) return ;

          _supportTargets.Add( positionTarget ) ;
        }
      }
      else {
        // 非単独
        _supportTargets.Clear() ;
        if ( null == positionTarget ) return ;

        _supportTargets.Add( positionTarget ) ;
        // それ以外を収集
        _supportTargets.UnionWith( CollectTargets( _supportPosition.Value.Position, positionTarget ) ) ;
      }
    }

    private IEnumerable<LeafEdge> CollectTargets( Vector3d position, LeafEdge target )
    {
      var pipe = target.PipingPiece as IParallelPipe ;
      if ( null == pipe ) yield break ;

      var bp = target.Closest<ISupportParentElement>() as Edge ;

      var globalPos = position ;
      if ( null != bp ) {
        globalPos = bp.GlobalCod.GlobalizePoint( globalPos ) ;
      }

      var pipeCod = target.GlobalCod ;
      var pipeDir1 = pipeCod.GlobalizeVector( pipe.LocalParallelDirection ) ;

      if ( pipeDir1.IsParallelTo( new Vector3d( 0, 0, 1 ) ) ) {
        // 垂直パイプの場合は無視
        yield break ;
      }

      foreach ( var edge in ( (IGroup) bp ?? Document ).GetAllLeafEdgesWithoutSubBlockPattern() ) {
        if ( edge == target ) continue ;

        var pipe2 = edge.PipingPiece as IParallelPipe ;
        if ( null == pipe2 ) continue ;

        var pipeCod2 = edge.GlobalCod ;
        var pipeDir2 = pipeCod2.GlobalizeVector( pipe.LocalParallelDirection ) ;
        if ( ! pipeDir1.IsParallelTo( pipeDir2 ) ) {
          // パイプ同士が平行ではない
          continue ;
        }

        var globalDistanceVec = pipeCod2.Origin - globalPos ;

        if ( ( pipe.ParallelDiameter + pipe2.ParallelDiameter ) / 2 <= Math.Abs( globalDistanceVec.z ) ) {
          // 高さ方向が離れすぎている
          continue ;
        }

        var parallelDirLength = Vector3d.Dot( pipeDir2, globalDistanceVec ) ;
        if ( ! pipe2.ParallelRanges.Any( range => range.Item1 <= parallelDirLength && parallelDirLength <= range.Item2 ) ) {
          // パイプの伸びる方向が離れすぎている
          continue ;
        }

        double dist2 = globalDistanceVec.sqrMagnitude - parallelDirLength * parallelDirLength ;
        double distance = ( dist2 < 0d ) ? 0d : Math.Sqrt( dist2 ) ;

        if ( ! CanSupportBoth( pipe, pipe2, distance ) ) {
          continue ;
        }

        yield return edge ;
      }
    }

    private bool CanSupportBoth( IParallelPipe pipe1, IParallelPipe pipe2, double distance )
    {
      var maxDiameter = Math.Max( pipe1.ParallelDiameter, pipe2.ParallelDiameter ) ;

      var length = GetTTypeSupportMaxShoulderLength( maxDiameter ) ;
      return ( distance + ( pipe1.ParallelDiameter + pipe2.ParallelDiameter ) / 2 ) <= length ;
    }

    public static double GetTTypeSupportMaxShoulderLength( double diameter )
    {
      if ( diameter.ToInches() <= 6.0 - Tolerance.DistanceTolerance ) {
        return ( 500.0 ).Millimeters() ;
      }
      else {
        return ( 1000.0 ).Millimeters() ;
      }
    }

    private bool NeedSeekTargets( SupportType type )
    {
      switch ( type ) {
        case SupportType.TType :
          return true ;

        default :
          return false ;
      }
    }

    public override Bounds? GetGlobalBounds()
    {
      // TEST
      if ( Document == Parent ) {
        return new Bounds( (Vector3) _supportPosition.Value.Position, Vector3.zero ) ;
      }
      else {
        var pos = (Vector3) ( (BlockPattern) Parent ).GlobalCod.GlobalizePoint( _supportPosition.Value.Position ) ;
        return new Bounds( pos, Vector3.zero ) ;
      }
    }

    public void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      _supportDirection.Value = _supportDirection.Value.MirrorVectorBy( normalDirection ) ;
      _supportPosition.Value.Mirror( origin, normalDirection ) ;
    }
  }
}