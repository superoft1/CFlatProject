using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Chiyoda;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace MTO
{
  public class Serializer
  {
    private readonly Document _doc = null;
 
    public Serializer( Document doc )
    {
      _doc = doc;
    }

    public void ExportData( string path )
    {
      if ( File.Exists( path ) ) {
        File.Delete( path );
      }

      var pipingLines = new List<PipingLine>();
      RouteComponents( pipingLines );
      CompositeBlockPatternComponents( pipingLines );

      using ( var writer = new StreamWriter( path, true ) )
      {
        var str = new StringBuilder();

        str.Append( "Area,LineNo,Bran,Spec,Insu,Shrt,Size1,Size2,Qty,Wgt,Axis,Ref,Pnt1,Pnt2,Pnt3,Pnt4\n" );

        foreach ( var pipingLine in pipingLines ) {
          foreach ( var ( components, branchIndex ) in pipingLine.Components.Select( ( components, index ) => ( components, index ) ) ) {
            foreach ( var component in components ) {
              str.Append( $"{pipingLine.AreaId}," );
              str.Append( $"{pipingLine.LineId}," );

              str.Append( $"B{branchIndex + 1}," );

              str.Append( $"{pipingLine.ServiceClass}," );
              str.Append( $"{pipingLine.InsulationType}," );

              str.Append( $"{ShortCodeTable.ToString( component )}," ); 

              var diameters = component.Diameters.Select( diameter => Round( diameter ) ).ToArray();
              Array.Resize( ref diameters, 2 );
              str.Append( $"{string.Join( ",", diameters )}," );

              str.Append( component.Length == 0.0 ? "," : $"{Round( component.Length )}," );
              str.Append( component.Weight == 0.0 ? "," : $"{Round( component.Weight )}," );

              if ( component.IsAxisymmetric ) {
                str.Append( ",," );
              }
              else {
                str.Append( $"{Round( component.Axis )}," );
                str.Append( $"{Round( component.Reference )}," );
              }

              var connectPoints = component.ConnectPoints.Select( connectPoint => Round( connectPoint ) ).ToArray();
              Array.Resize( ref connectPoints, 4 );
              str.Append( $"{string.Join( ",", connectPoints )}\n" );
            }
          }
        }

        writer.Write( str );
      }
    }

    private void RouteComponents( List<PipingLine> pipingLines )
    {
      foreach ( var route in _doc.Routes ) {
        if ( route.HasError ) {
          continue;
        }

//        AttachFlowType( route ); // Note: FlowTypeが設定されていない状況に対する暫定対応

        var leafEdges = route.Children.OfType<LeafEdge>();
        var routeComponents = new List<List<IPipingComponent>>();
        var duplicateChecker = new HashSet<LeafEdge>();
        foreach ( var leafEdge in CollectRootLeafEdges( leafEdges ) ) {
          TrailComponents( leafEdge, leafEdges, routeComponents, duplicateChecker );
        }

        if ( routeComponents.Any() ) {
          var pipingLine = new PipingLine() { LineId = route.LineId, InsulationType = route.InsulationType };
          pipingLine.Components.AddRange( routeComponents );
          pipingLines.Add( pipingLine );
        }
      }
    }

    private void CompositeBlockPatternComponents( List<PipingLine> pipingLines )
    {
      foreach ( var compositeBlockPatterns in _doc.EdgeList.OfType<CompositeBlockPattern>() ) {
        compositeBlockPatterns.SetFlowType(); // Note: FlowTypeが設定されていない状況に対する暫定対応

        var leafEdges = compositeBlockPatterns.GetAllLeafEdges().Where( leafEdge => leafEdge.PipingPiece is Chiyoda.CAD.Model.Component );
        var compositeBlockPatternComponents = new List<List<IPipingComponent>>();
        var duplicateChecker = new HashSet<LeafEdge>();
        foreach ( var leafEdge in CollectRootLeafEdges( leafEdges ) ) {
          TrailComponents( leafEdge, leafEdges, compositeBlockPatternComponents, duplicateChecker );
        }

        if ( compositeBlockPatternComponents.Any() ) {
          var pipingLine = new PipingLine();
          pipingLine.Components.AddRange( compositeBlockPatternComponents );
          pipingLines.Add( pipingLine );
        }
      }
    }

    private void TrailComponents( LeafEdge leafEdge, IEnumerable<LeafEdge> leafEdges, List<List<IPipingComponent>> trailedComponents, HashSet<LeafEdge> duplicateChecker )
    {
      var branchLeafEdges = new Queue<LeafEdge>();
      branchLeafEdges.Enqueue( leafEdge );

      while ( branchLeafEdges.Any() ) {
        leafEdge = branchLeafEdges.Dequeue();

        var components = new List<IPipingComponent>();

        while ( null != leafEdge ) {
          var component = HasComponent( leafEdge );
          if ( null == component || !duplicateChecker.Add( leafEdge ) ) {
            break;
          }
          components.AddRange( component );

          var connectedLeafEdges = leafEdge.Vertices.Where( halfVertex => null != halfVertex.Partner &&
                                                                          halfVertex.Flow == HalfVertex.FlowType.FromThisToAnother )
                                                    .Select( halfVertex => halfVertex.Partner.LeafEdge )
                                                    .Where( connectedLeafEdge => leafEdges.Contains( connectedLeafEdge ) );
          leafEdge = connectedLeafEdges.FirstOrDefault();
          foreach ( var connectedLeafEdge in connectedLeafEdges.Skip( 1 ) ) {
            branchLeafEdges.Enqueue( connectedLeafEdge );
          }
        }

        if ( components.Any() ) {
          trailedComponents.Add( components );
        }
      }
    }

    private IEnumerable<IPipingComponent> HasComponent( LeafEdge leafEdge )
    {
      var cod = leafEdge.GlobalCod;

      switch ( leafEdge.PipingPiece ) {
        case GateValve gateValve:
          return new [] { new GateValveComponent( gateValve ) };
////        case GlobeValve _:
        case BallValve ballValve:
          return new [] { new BallValveComponent( ballValve ) };
        case ButterflyValve butterflyValve:
          return new [] { new ButterflyValveComponent( butterflyValve ) };
////        case ThreeWayInstrumentRootvalve _:
        case CheckValve checkValve:
          return new [] { new CheckValveComponent( checkValve ) };
////        case SafetyValve _:

        case ControlValve controlValve:
          return new [] { new ControlValveComponent( controlValve, cod ) };
////        case GraduatedControlValve _:
////        case InstrumentAngleControlValve _:

        case Pipe pipe:
          return new [] { new PipeComponent( pipe ) };
        case PipingElbow45 elbow45:
          return new [] { new PipingElbow45Component( elbow45, cod ) };
        case PipingElbow90 elbow90:
          return new [] { new PipingElbow90Component( elbow90, cod ) };
////        case PipeBend _:
////        case PipingCap _:
////        case PipingCoupling _:
////        case PipingPlug _:

        case PipingTee tee:
          return new [] { new PipingTeeComponent( tee, cod ) };
////        case PipingLateralTee _:
////        case SockOlet _:
////        case WeldOlet _:
////        case StubInReinforcingWeld _:

        case ConcentricPipingReducerCombination concentricReducer:
          return ConcentricPipingReducerComponent.Create( concentricReducer, cod );
        case EccentricPipingReducerCombination eccentricReducer:
          return EccentricPipingReducerComponent.Create( eccentricReducer, cod );

////        case Flange _:
////        case SlipOnFlange _:
        case WeldNeckFlange weldNeckFlange:
          return new [] { new WeldNeckFlangeComponent( weldNeckFlange, cod ) };
        case BlindFlange blindFlange:
          return new [] { new BlindFlangeComponent( blindFlange, cod ) };

////        case YStrainer _:
////        case AngleTStrainer _:
////        case InLineTStrainer _:

        case OpenSpectacleBlank openSpectacleBlank:
          return new [] { new OpenSpectacleBlankComponent( openSpectacleBlank ) };
        case BlankSpectacleBlank blankSpectacleBlank:
          return new [] { new BlankSpectacleBlankComponent( blankSpectacleBlank ) };
////        case OrificePlate _:
////        case RestrictorPlate _:
      }

      Debug.LogError( $"Error: An unsupported type was detected.: {leafEdge.PipingPiece.GetType().Name}" );
      return null;
    }

    private IEnumerable<LeafEdge> CollectRootLeafEdges( IEnumerable<LeafEdge> leafEdges )
    {
      var rootLeafEdges = new HashSet<LeafEdge>();

      foreach ( var leafEdge in leafEdges ) {
        foreach ( var rootLeafEdge in FindRootLeafEdge( leafEdge, leafEdges ) ) {
          rootLeafEdges.Add( rootLeafEdge );
        }
      }

      return rootLeafEdges;
    }

    private IEnumerable<LeafEdge> FindRootLeafEdge( LeafEdge leafEdge, IEnumerable<LeafEdge> leafEdges )
    {
      bool foundConnection = false;

      var connectedLeafEdges = leafEdge.Vertices.Where( halfVertex => null != halfVertex.Partner &&
                                                                      halfVertex.Flow == HalfVertex.FlowType.FromAnotherToThis )
                                                .Select( halfVertex => halfVertex.Partner.LeafEdge )
                                                .Where( connectedLeafEdge => leafEdges.Contains( connectedLeafEdge ) );
      foreach ( var connectedLeafEdge in connectedLeafEdges ) {
        foreach ( var rootLeafEdge in FindRootLeafEdge( connectedLeafEdge, leafEdges ) ) {
          yield return rootLeafEdge;
        }
        foundConnection = true;
      }

      if ( !foundConnection ) {
        yield return leafEdge;
      }
    }

    private string Round( double value )
    {
      return Math.Round( value, 2, MidpointRounding.AwayFromZero ).ToString( "0.00" );
    }

    private string Round( Vector3d value )
    {
      return $"{Round( value.x )} {Round( -value.y )} {Round( value.z )}";
    }

    private void AttachFlowType( Route route )
    {
      var leafEdges = route.Children.OfType<LeafEdge>();
      if ( !leafEdges.Any() ) {
        return;
      }

      if ( route.FromPoint == null ) {
        Debug.LogError( $"Error: From point is null.: {route.LineId}" );
        return;
      }
      if ( route.Branches.Any( branch => HasStartPoint( branch ) ) ) {
        Debug.LogError( $"Error: Multiple from points are not supported.: {route.LineId}" );
        return;
      }
      AttachFlowType( route.FromPoint.LinkPoint, leafEdges );
    }

    private bool HasStartPoint( IBranch branch )
    {
      return branch.IsStart || branch.Branches.Any( branch_ => HasStartPoint( branch_ ) );
    }

    private void AttachFlowType( HalfVertex fromVertex, IEnumerable<LeafEdge> leafEdges )
    {
      var fromVertices = new Queue<HalfVertex>();
      fromVertices.Enqueue( fromVertex );

      while ( fromVertices.Any() ) {
        fromVertex = fromVertices.Dequeue();

        while ( null != fromVertex ) {
          var toVertex = fromVertex.Partner;
          if ( null == toVertex ) {
            break;
          }

          fromVertex.Flow = HalfVertex.FlowType.FromThisToAnother;
          toVertex.Flow = HalfVertex.FlowType.FromAnotherToThis;

          var connectedLeafEdge = toVertex.LeafEdge;
          if ( !leafEdges.Contains( connectedLeafEdge ) ) {
            break;
          }

          var nextVertices = connectedLeafEdge.Vertices.Where( halfVertex => null != halfVertex.Partner &&
                                                                             halfVertex.Flow == HalfVertex.FlowType.Undefined );
          fromVertex = nextVertices.FirstOrDefault();
          foreach ( var nextVertex in nextVertices.Skip( 1 ) ) {
            fromVertices.Enqueue( nextVertex );
          }
        }
      }
    }
  }
}
