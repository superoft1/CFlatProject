using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Maintainer
{
  public class ConnectionMaintainer
  {
    private readonly Edge _ownerEdge ;
    private readonly LeafEdge _leafEdge ;
    private readonly LocalCodSys3d _baseCodSys ;
    private readonly HashSet<LeafEdge> _maintainedEdges = new HashSet<LeafEdge>() ;

    private FlexTracker _trackerX ;
    private FlexTracker _trackerY ;
    private FlexTracker _trackerZ ;

    public ConnectionMaintainer( LeafEdge leafEdge )
    {
      _leafEdge = leafEdge ;
      _ownerEdge = GetOwnerEdge( _leafEdge ) ;
      if ( null == _ownerEdge ) throw new InvalidOperationException( $"`{leafEdge}` has no BlockPattern ancestors." ) ;
      _baseCodSys = _ownerEdge?.GlobalCod ?? LocalCodSys3d.Identity ;
    }

    private static BlockEdge GetOwnerEdge( Edge edge )
    {
      var bp = edge?.Closest<BlockEdge>() ;
      if ( bp is CompositeBlockPattern ) return null ;

      while ( null != bp ) {
        var next = bp.Parent?.Closest<BlockEdge>() ;
        if ( null == next ) return bp ;

        bp = next ;
      }

      return null ;
    }

    private static bool IsBreakEdge( LeafEdge edge )
    {
      return ( edge.PipingPiece is Equipment ) ;
    }

    public void MaintainConnections()
    {
      _maintainedEdges.Clear() ;
      _maintainedEdges.Add( _leafEdge ) ;
      
      HashSet<HalfVertex> returnedVertices = null ;
      foreach ( var v in _leafEdge.Vertices ) {
        if ( null != returnedVertices && returnedVertices.Contains( v ) ) continue ;
        _trackerX = null ;
        _trackerY = null ;
        _trackerZ = null ;
        foreach ( var end in Maintain( v ) ) {
          if ( end.LeafEdge == _leafEdge ) {
            if ( null == returnedVertices ) returnedVertices = new HashSet<HalfVertex>() ;
            returnedVertices.Add( end ) ;
          }
        }
      }
    }

#if DEBUG
    private int _maintainLoopCount = 0 ;
#endif

    private IEnumerable<HalfVertex> Maintain( HalfVertex trackingVertex )
    {
      var another = trackingVertex.Partner ;
      if ( another == null ) yield break ;

      var globalDiff = trackingVertex.GlobalPoint - another.GlobalPoint ;
      var diff = _baseCodSys.LocalizeVector( globalDiff ) ;
      var anotherLeafEdge = another.LeafEdge ;
      if ( IsBreakEdge( anotherLeafEdge ) || _ownerEdge != GetOwnerEdge( anotherLeafEdge ) || _maintainedEdges.Contains( anotherLeafEdge ) ) {
        TrackX( diff.x, anotherLeafEdge, false ) ;
        TrackY( diff.y, anotherLeafEdge, false ) ;
        TrackZ( diff.z, anotherLeafEdge, false ) ;
        _maintainedEdges.Add( anotherLeafEdge ) ;
        yield return another;
        yield break ;
      }

      if ( PositionMode.FixedX == ( PositionMode.FixedX & anotherLeafEdge.PositionMode ) ) {
        TrackX( diff.x, anotherLeafEdge, true ) ;
        globalDiff = trackingVertex.GlobalPoint - another.GlobalPoint ;
        diff = _baseCodSys.LocalizeVector( globalDiff ) ;
      }

      if ( PositionMode.FixedY == ( PositionMode.FixedY & anotherLeafEdge.PositionMode ) ) {
        TrackY( diff.y, anotherLeafEdge, true ) ;
        globalDiff = trackingVertex.GlobalPoint - another.GlobalPoint ;
        diff = _baseCodSys.LocalizeVector( globalDiff ) ;
      }

      if ( PositionMode.FixedZ == ( PositionMode.FixedZ & anotherLeafEdge.PositionMode ) ) {
        TrackZ( diff.z, anotherLeafEdge, true ) ;
        globalDiff = trackingVertex.GlobalPoint - another.GlobalPoint ;
        diff = _baseCodSys.LocalizeVector( globalDiff ) ;
      }

      AddTrackingPiece( trackingVertex, another, anotherLeafEdge ) ;
      anotherLeafEdge.Translate( globalDiff ) ;

      _maintainedEdges.Add( anotherLeafEdge ) ;
      HashSet<HalfVertex> returnedVertices = null ;
      foreach ( var v in anotherLeafEdge.Vertices ) {
        if ( v == another ) continue ;
        if ( null != returnedVertices && returnedVertices.Contains( v ) ) continue ;

#if DEBUG
        ++_maintainLoopCount ;
        if ( 1024 <= _maintainLoopCount ) throw new InvalidOperationException() ;
#endif
        foreach ( var end in Maintain( v ) ) {
          if ( end.LeafEdge == anotherLeafEdge ) {
            if ( null == returnedVertices ) returnedVertices = new HashSet<HalfVertex>() ;
            returnedVertices.Add( end ) ;
          }

          yield return end ;
        }
#if DEBUG
        --_maintainLoopCount ;
#endif
      }
    }

    private void TrackX( double diffX, LeafEdge toLeafEdge, bool moveToLeafEdgeIfNeeded )
    {
      _trackerX?.Solve( diffX, toLeafEdge, moveToLeafEdgeIfNeeded ) ;
      _trackerX = null ;
    }

    private void TrackY( double diffY, LeafEdge toLeafEdge, bool moveToLeafEdgeIfNeeded )
    {
      _trackerY?.Solve( diffY, toLeafEdge, moveToLeafEdgeIfNeeded ) ;
      _trackerY = null ;
    }

    private void TrackZ( double diffZ, LeafEdge toLeafEdge, bool moveToLeafEdgeIfNeeded )
    {
      _trackerZ?.Solve( diffZ, toLeafEdge, moveToLeafEdgeIfNeeded ) ;
      _trackerZ = null ;
    }

    private void AddTrackingPiece( HalfVertex trackingVertex, HalfVertex fromVertex, LeafEdge edge )
    {
      var pipe = edge.PipingPiece as Pipe ;
      if ( null == pipe ) return ;

      var dir = edge.LocalCod.GlobalizeVector( pipe.Axis ) ;
      if ( fromVertex.ConnectPointIndex == (int) Pipe.ConnectPointType.Term2 ) dir = -dir ;

      switch ( GetDirection( dir ) ) {
        case Direction.XMinus :
          if ( null == _trackerX ) _trackerX = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerX.AddPipe( edge, false ) ;
          break ;
        case Direction.XPlus :
          if ( null == _trackerX ) _trackerX = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerX.AddPipe( edge, true ) ;
          break ;

        case Direction.YMinus :
          if ( null == _trackerY ) _trackerY = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerY.AddPipe( edge, false ) ;
          break ;
        case Direction.YPlus :
          if ( null == _trackerY ) _trackerY = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerY.AddPipe( edge, true ) ;
          break ;

        case Direction.ZMinus :
          if ( null == _trackerZ ) _trackerZ = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerZ.AddPipe( edge, false ) ;
          break ;
        case Direction.ZPlus :
          if ( null == _trackerZ ) _trackerZ = new FlexTracker( trackingVertex, _maintainedEdges ) ;
          _trackerZ.AddPipe( edge, true ) ;
          break ;

        default : break ;
      }
    }

    private static Direction GetDirection( Vector3d vec )
    {
      var keps2 = 1e-6 ;
      double x2 = vec.x * vec.x, y2 = vec.y * vec.y, z2 = vec.z * vec.z ;

      if ( ( y2 + z2 ) < x2 * keps2 ) {
        return ( 0 < vec.x ) ? Direction.XPlus : Direction.XMinus ;
      }

      if ( ( z2 + x2 ) < y2 * keps2 ) {
        return ( 0 < vec.y ) ? Direction.YPlus : Direction.YMinus ;
      }

      if ( ( x2 + y2 ) < z2 * keps2 ) {
        return ( 0 < vec.z ) ? Direction.ZPlus : Direction.ZMinus ;
      }

      return Direction.Others ;
    }

    private enum Direction
    {
      XPlus,
      XMinus,
      YPlus,
      YMinus,
      ZPlus,
      ZMinus,
      Others,
    }

    private class FlexTracker
    {
      private readonly struct PipeWithDirection
      {
        public Pipe Pipe { get ; }
        public double FlexRatio { get ; }

        public PipeWithDirection( Pipe pipe, double flexRatio )
        {
          Pipe = pipe ;
          FlexRatio = flexRatio ;
        }
      }

      private readonly HalfVertex _trackingStartVertex ;
      private readonly BlockEdge _trackingStartBlock ;
      private readonly HashSet<LeafEdge> _maintainedEdges ;
      private readonly List<PipeWithDirection> _flexers = new List<PipeWithDirection>() ;

      public FlexTracker( HalfVertex trackingStartVertex, HashSet<LeafEdge> maintainedEdges )
      {
        _trackingStartVertex = trackingStartVertex ;
        _trackingStartBlock = GetOwnerEdge( trackingStartVertex.LeafEdge ) ;
        _maintainedEdges = maintainedEdges ;
      }

      public void AddPipe( LeafEdge leafEdge, bool isForward )
      {
        var flexRatio = Manager.PipeManager.GetFlexRatio( leafEdge ) ;
        if ( 0 == flexRatio ) return ;

        _flexers.Add( new PipeWithDirection( leafEdge.PipingPiece as Pipe, ( isForward ? flexRatio : -flexRatio ) ) ) ;
      }

      public void Solve( double difference, LeafEdge endOfTracking, bool moveToLeafEdgeIfNeeded )
      {
        if ( Math.Abs( difference ) < 1e-6 ) return ;

        double sumFlex = _flexers.Sum( x => x.FlexRatio ) ;
        if ( sumFlex == 0 ) return ;

        SolveFlexFrom( _trackingStartVertex, endOfTracking, 0, difference / Math.Abs( sumFlex ), moveToLeafEdgeIfNeeded ) ;
      }


#if DEBUG
      private readonly HashSet<HalfVertex> _invalidLoopChecker = new HashSet<HalfVertex>() ;
#endif

      private void SolveFlexFrom( HalfVertex trackingVertex, LeafEdge toLeafEdge, int nextFlexerIndex, double flexValue, bool moveToLeafEdgeIfNeeded )
      {
        var another = trackingVertex.Partner ;
        if ( another == null ) return ;

        var anotherLeafEdge = another.LeafEdge ;
        if ( anotherLeafEdge.PipingPiece is Pipe pipe && nextFlexerIndex < _flexers.Count && _flexers[ nextFlexerIndex ].Pipe == pipe ) {
          pipe.PreferredLength -= flexValue * _flexers[ nextFlexerIndex ].FlexRatio ;
          ++nextFlexerIndex ;
        }

        var anotherBlock = GetOwnerEdge( anotherLeafEdge ) ;
        if ( IsBreakEdge( another.LeafEdge ) || _trackingStartBlock != anotherBlock || false == _maintainedEdges.Contains( anotherLeafEdge ) ) {
          // 追尾範囲の外に出たなら終了
          foreach ( var cbp in GetAncestorCompositeBlockPatterns( anotherLeafEdge ) ) {
            another.Document.RegisterJointEdgeMoving( cbp ) ;
          }

          return ;
        }

        if ( anotherLeafEdge == toLeafEdge ) {
          if ( moveToLeafEdgeIfNeeded ) {
            anotherLeafEdge.Translate( trackingVertex.GlobalPoint - another.GlobalPoint ) ;
          }
          return ;
        }

        anotherLeafEdge.Translate( trackingVertex.GlobalPoint - another.GlobalPoint ) ;

        foreach ( var v in anotherLeafEdge.Vertices ) {
          if ( v == another ) continue ;

#if DEBUG
          if ( ! _invalidLoopChecker.Add( v ) ) throw new InvalidOperationException() ;
#endif
          SolveFlexFrom( v, toLeafEdge, nextFlexerIndex, flexValue, moveToLeafEdgeIfNeeded ) ;
#if DEBUG
          _invalidLoopChecker.Remove( v ) ;
#endif
        }
      }

      private static IEnumerable<CompositeBlockPattern> GetAncestorCompositeBlockPatterns( LeafEdge edge )
      {
        var cbp = edge.Closest<CompositeBlockPattern>() ;
        while ( null != cbp ) {
          yield return cbp ;
          cbp = cbp.Parent?.Closest<CompositeBlockPattern>() ;
        }
      }
    }
  }
}