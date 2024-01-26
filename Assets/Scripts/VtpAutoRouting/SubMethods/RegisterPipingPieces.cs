using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using Routing ;
using Routing.Process ;
using Routing.Process.HelperFuncs ;
using UnityEngine ;
using UnityEngine.Experimental.Rendering ;
using VtpAutoRouting.BridgeEntities ;

namespace VtpAutoRouting.SubMethods
{
  internal class RegistrationError
  {
    private static float MinPipeLength = 0.1f ;

    private class TwoPartsError : IRegistrationError
    {
      private readonly List<IRouteVertex> _errors = new List<IRouteVertex>() ;

      public TwoPartsError( string message, IRouteVertex from, IRouteVertex to )
      {
        _errors.Add( from ) ;
        _errors.Add( to ) ;
        Message = message ;
      }

      public string Message { get ; }

      public IEnumerable<IRouteVertex> ErrorPositions => _errors ;
    }

    private class Error : IRegistrationError
    {
      public Error( string message )
      {
        Message = message ;
      }

      public string Message { get ; }
      public IEnumerable<IRouteVertex> ErrorPositions => Enumerable.Empty<IRouteVertex>() ;
    }

    public static IRegistrationError GeneralError( string message = "Unknown" ) => new Error( message ) ;

    public static IRegistrationError InterferedParts( IRouteVertex p1, IRouteVertex p2 )
      => new TwoPartsError( "Interfered", p1, p2 ) ;

    public static IRegistrationError InclinedPipeError( IRouteVertex p1, IRouteVertex p2 )
      => new TwoPartsError( "Inclined Pipe", p1, p2 ) ;
  }

  internal static class RegisterPipingPieces
  {
    public static RegistrationErrors Execute( Route route, IAutoRoutingResult result )
    {
      var errors = new RegistrationErrors();
      if ( result == null ) {
        return errors ;
      }

      var parts = new List<LeafEdge>() ;
      
      foreach ( var (node, isError) in CreatePipingPieces( route.Document, result, errors ) ) {
        route.PutRoutingResult( node, isError ) ;
        parts.Add( node );
      }
      route.SetThroughPoints( result.ThroughPoints );

      var vertices = route.GetAllTerminalVertices().ToList() ;      
      if ( ! AutoRoutingTopologyExt.Connect( parts, vertices ) ) {
        route.ErrorMessage = "topology error" + route.ErrorMessage ;
      }

      if ( parts.Count < 1 ) {
        return errors ;
      }

      if ( !FlowInspection.Run( route, parts ) ) {
        Debug.LogError( $"[AutoRouting] Found Flow Direction Error in {route.LineId}" );
      }

      return errors ;
    }    
    
    private static IEnumerable<(LeafEdge, bool)> CreatePipingPieces(
       Chiyoda.CAD.Core.Document doc, IAutoRoutingResult result, RegistrationErrors errors )
    {
      var leafEdges = new Dictionary<IRouteVertex, LeafEdge>() ;
      result.RouteVertices
        .Select( v => ( v, ConvertToPipingPiece( doc, v ) ) )
        .Where( tuple => tuple.Item2 != null )
        .ForEach( tuple => leafEdges.Add( tuple.v, tuple.Item2 ) );
      
      var flowRoots = new List<(HalfVertex, int)>() ;
      void SetFlow( TerminalPoint p, HalfVertex v ) {
        v.Flow = p.IsStart
          ? HalfVertex.FlowType.FromAnotherToThis
          : HalfVertex.FlowType.FromThisToAnother ;
        flowRoots.Add( (v, p.Priority) );
      }

      var errorPositions = new HashSet<IRouteVertex>() ;
      var edges = new List<(LeafEdge, bool)>() ;
      foreach ( var e in result.RouteEdges ) {
        var (edge, error) = ConnectParts( doc, e, ( p, v ) => GetPositionVertex( leafEdges, p, v ), SetFlow ) ;

        if ( error != null ) {
          errors.Register( e.Start, error );
          error.ErrorPositions.ForEach( er => errorPositions.Add( er ) ) ;
        }
        if ( edge == null ) {
          continue ;
        }

        edges.Add( ( edge, error != null ) ) ;
      }

      BuildTopology.AttachFlow( flowRoots );
      return leafEdges
        .Select( tuple => ( tuple.Value, errorPositions.Contains( tuple.Key ) ) )
        .Concat( edges ) ;
    }

    
    private static (LeafEdge, IRegistrationError) ConnectParts( 
      Chiyoda.CAD.Core.Document doc, IRouteEdge e,
      Func<IRouteVertex, Vector3d, HalfVertex> findHalfVertex,
      Action<TerminalPoint, HalfVertex> setFlow )
    {
      void TrySetFlow( IRouteVertex routeVtx, HalfVertex v ) 
      {
        if ( routeVtx is TerminalPoint t ) {
          setFlow( t, v ) ;
        }
      }

      var dir = (e.End.Position - e.Start.Position).To3d() ;
      if ( Math.Abs( dir.magnitude ) < 0.001f ) {
        return (null, RegistrationError.GeneralError("[AutoRouting] Too close check points" ) );
      }

      var startVtx = findHalfVertex( e.Start, dir ) ;
      var endVtx = findHalfVertex( e.End, -dir ) ;

      var (startPos, endPos) = CalcTermPoints( e.Start, e.End, startVtx, endVtx ) ;

      var pipeDir = endPos - startPos ;
      if ( pipeDir.magnitude < 0.05 ) {
        ConnectVertices( startVtx, endVtx );
        if ( endVtx != null ) TrySetFlow( e.Start, endVtx );
        if ( startVtx != null ) TrySetFlow( e.End, startVtx );
        return (null, null) ;
      }
      
      if ( Vector3d.Dot( dir.normalized, pipeDir ) < 0.01 ) {
        ConnectVertices( startVtx, endVtx ) ;
        return (null, RegistrationError.InterferedParts( e.Start, e.End ) ) ;
      }

      var diameter = Extensions.GetMin( e.Start.PipeProperty, e.End.PipeProperty ) ;
      var error = pipeDir.IsAxisAligned()
        ? null
        : RegistrationError.InclinedPipeError( e.Start, e.End ) ;

      var edge = RegisterPipe( doc, startPos, endPos, diameter ) ;
      var pipeStart = edge.GetVertex( 0 ) ;
      var pipeEnd = edge.GetVertex( 1 ) ;
      ConnectVertices( startVtx, pipeStart ) ;
      ConnectVertices( endVtx, pipeEnd ) ;
      TrySetFlow( e.Start, pipeStart );
      TrySetFlow( e.End, pipeEnd );
      return (edge, error) ;
    }    
    
    private static HalfVertex GetPositionVertex( 
      IDictionary<IRouteVertex, LeafEdge> pipingPieces,
      IRouteVertex p, Vector3d dir )
    {
      if ( ! pipingPieces.TryGetValue( p, out var startLeaf ) ) {
        return null ;
      }
      var startVtx = GetHalfVertex( startLeaf, dir ) ;
      if ( startVtx != null ) {
        return startVtx ;
      }
      Debug.Log( "[AutoRouting] Topology Error" ) ;
      return null ;
    }

    private static HalfVertex GetHalfVertex( Edge edge, Vector3d dir )
    {
      return edge?.Vertices.Where( v => v.Partner == null )
        .OrderByDescending( v => Vector3d.Dot( ( v.GlobalPoint - edge.GlobalCod.Origin ).normalized, dir ) )
        .First() ;
    }
    
    private static bool ConnectVertices( HalfVertex startVtx, HalfVertex endVtx )
    {
      if ( ( startVtx == null ) || ( endVtx == null ) ) {
        return false ;
      }
      
      if ( IsEmptyPartner( startVtx ) && IsEmptyPartner( endVtx ) ) {
        startVtx.Partner = endVtx ;
        return true ;
      }
      Debug.LogWarning( "[AutoRouting] HalfVertex Connection Failed" );
      return false ;
    }

    private static (Vector3d start, Vector3d end) CalcTermPoints( 
      IRouteVertex node, IRouteVertex next, HalfVertex startVtx, HalfVertex endVtx )
    {
      var startPos = startVtx?.GlobalPoint ?? node.Position.To3d() ;
      var endPos = endVtx?.GlobalPoint ?? next.Position.To3d() ;
      
      var diffD = Math.Abs( node.PipeProperty.Outside - next.PipeProperty.Outside ) ;
      var diffZ = Math.Abs( startPos.z - endPos.z ) ;
      if ( ! ( diffD > Chiyoda.CAD.Core.Tolerance.FloatEpsilon ) || ! ( Math.Abs( 0.5 * diffD - diffZ ) < 0.01 ) ) {
        return (startPos, endPos) ;
      }

      //EccReducer Case
      return (node.PipeProperty.NPSmm > next.PipeProperty.NPSmm )
        ? ( new Vector3d( startPos.x, startPos.y, endPos.z ), endPos )
        : ( startPos, new Vector3d( endPos.x, endPos.y, startPos.z ) ) ;
    }
    
    private static bool IsEmptyPartner( HalfVertex v )
    {
      if ( v == null ) {
        return false ;
      }

      if ( v.Partner == null ) {
        return true ;
      }

      Debug.Log( "[AutoRouting] Topology Error" ) ;
      return false ;
    }
    

    private static LeafEdge ConvertToPipingPiece(
      Chiyoda.CAD.Core.Document doc, IRouteVertex e )
    {
      if ( e is Corner c ) {
        return RegisterElbow( doc, c ) ;
      }

      if ( e is Branch b ) {
        return RegisterBranch( doc, b ) ;
      }
      return null ;
    }
    private static LeafEdge RegisterElbow( Chiyoda.CAD.Core.Document doc, Corner corner )
    {
      var elbow = doc.CreateEntity<Chiyoda.CAD.Model.PipingElbow90>() ;
      elbow.ChangeSizeNpsMm( 0, corner.PipeProperty.NPSmm ) ;

      var leaf = doc.CreateLeafEdge( elbow ) ;
      LeafEdgeCodSysUtils.LocalizeElbow90Component( leaf, corner.Position.To3d(), corner.Direction0.To3d(), -corner.Direction1.To3d() ) ;
      return leaf ;
    }

    private static LeafEdge RegisterBranch( Chiyoda.CAD.Core.Document doc, Branch b )
    {
      var mainDiameter = b.PipeProperty.Outside ;
      var subDiameter = b.BranchProperty.Outside ;
      var tee = doc.CreateEntity<Chiyoda.CAD.Model.PipingTee>() ;
      tee.MainDiameter = mainDiameter ;
      tee.MainLength = subDiameter + 2.0f * mainDiameter ;
      tee.BranchDiameter = subDiameter ;
      tee.BranchLength = 0.5 * mainDiameter + subDiameter ;

      var leaf = doc.CreateLeafEdge( tee ) ;
      LeafEdgeCodSysUtils.LocalizeTeeComponent( leaf, b.Position.To3d(), b.MainDirection.To3d(), -b.BranchDirection.To3d() ) ;
      return leaf ;
    }

    private static LeafEdge RegisterPipe( Chiyoda.CAD.Core.Document doc, Vector3d start, Vector3d end, IPipeProperty prop )
    {
      var lineDir = end - start ;
      var pipe = doc.CreateEntity<Chiyoda.CAD.Model.Pipe>() ;
      pipe.Diameter = prop.Outside ;
      pipe.Length = lineDir.magnitude ;

      var leaf = doc.CreateLeafEdge( pipe ) ;
      LeafEdgeCodSysUtils.LocalizeStraightComponent( leaf, 0.50 * ( start + end ), lineDir ) ;
      return leaf ;
    }

    
  }
}