using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Importer.Checker;
using UnityEngine;

namespace IDF
{
  class LineAnalysis
  {
    private readonly Document _doc = null;
    private readonly string[] _lineIDs = null;

    public LineAnalysis( Document doc, string[] lineIDs )
    {
      _doc = doc;

      _lineIDs = new string[lineIDs.Length];
      lineIDs.CopyTo( _lineIDs, 0 );
    }

    public void LineComponents( List<PipingLine> pipingLines )
    {
      foreach ( var line in _doc.Lines ) {
        var lineId = line.LineId;
        var serviceClass = line.ServiceClass;

        if ( !_lineIDs.Contains( lineId ) ) {
          continue;
        }

        var pipingComponents = new List<PipingComponent>();
        var duplicateChecker = new HashSet<Chiyoda.CAD.Model.Component>();
        foreach ( var leafEdge in CollectRootLeafEdges( line ) ) {
          LineComponents( leafEdge, line, pipingComponents, duplicateChecker );
        }

        if ( pipingComponents.Any() ) {
          var pipingLine = new PipingLine() { LineID = lineId, Specification = serviceClass };
          pipingLine.Components.AddRange( pipingComponents );
          pipingLines.Add( pipingLine );
        }
      }
    }

    public void OutputComponentTables()
    {
      var path = Application.dataPath + "/Outputs/IDFComponentTable.csv";
      if ( System.IO.File.Exists( path ) ) {
        System.IO.File.Delete( path );
      }

      var pipingLines = new List<PipingLine>();
      LineComponents( pipingLines );

      using ( var writer = new StreamWriter( path, true ) )
      {
        var str = new StringBuilder();

        foreach ( var pipingLine in pipingLines ) {
          foreach ( var component in pipingLine.Components ) {
            str.Append( $"{pipingLine.LineID}, " );
            str.Append( $"{component.Type}, " );
            str.Append( $"{component.Tag}, " );
            str.Append( $"{string.Join( ", ", component.Diameters )}, " );
            str.Append( $"{pipingLine.Specification}\n" );
          }
        }

        writer.Write( str );
      }
    }

    private IEnumerable<LeafEdge> CollectRootLeafEdges( Line line )
    {
      var rootLeafEdges = new HashSet<LeafEdge>();

      foreach ( var leafEdge in line.LeafEdges ) {
        foreach ( var rootLeafEdge in FindRootLeafEdge( leafEdge, line ) ) {
          rootLeafEdges.Add( rootLeafEdge );
        }
      }

      return rootLeafEdges;
    }

    private IEnumerable<LeafEdge> FindRootLeafEdge( LeafEdge leafEdge, Line line )
    {
      bool foundConnection = false;

      foreach ( var halfVertex in leafEdge.Vertices ) {
        if ( null == halfVertex.Partner ) {
          continue;
        }

        switch ( halfVertex.Flow ) {
          case HalfVertex.FlowType.FromAnotherToThis:
            var connectedLeafEdge = halfVertex.Partner.LeafEdge;
            if ( line.LeafEdges.Contains( connectedLeafEdge ) ) {
              foreach ( var rootLeafEdge in FindRootLeafEdge( connectedLeafEdge, line ) ) {
                yield return rootLeafEdge;
              }
              foundConnection = true;
            }
            break;
          case HalfVertex.FlowType.Undefined:
            Debug.LogError( "Error: FlowType is undefined." );
            break;
        }
      }

      if ( !foundConnection ) {
        yield return leafEdge;
      }
    }

    private Chiyoda.CAD.Model.Component IsComponent( PipingPiece pipingPiece )
    {
      Chiyoda.CAD.Model.Component component = null;

      switch ( pipingPiece ) {
        case GateValve _:
        case GlobeValve _:
        case BallValve _:
        case ButterflyValve _:
        case CheckValve _:

        case PipingTee _:
        case PipingLateralTee _:
        case SockOlet _:
        case WeldOlet _:

        case ConcentricPipingReducerCombination _:
        case EccentricPipingReducerCombination _:

        case ControlValve _:
        case GraduatedControlValve _:
        case InstrumentAngleControlValve _:

        case YStrainer _:
        case AngleTStrainer _:
        case InLineTStrainer _:

        case OpenSpectacleBlank _:
        case BlankSpectacleBlank _:
        case OrificePlate _:
          component = (Chiyoda.CAD.Model.Component)pipingPiece;
          break;
      }

      return component;
    }

    private void LineComponents( LeafEdge leafEdge, Line line, List<PipingComponent> pipingComponents, HashSet<Chiyoda.CAD.Model.Component> duplicateChecker )
    {
      Chiyoda.CAD.Model.Component component = IsComponent( leafEdge.PipingPiece );
      if ( null != component && duplicateChecker.Add( component ) ) {
        var componentName = component.ComponentName;
        var stockNumber = component.StockNumber;
//        var diameter = component.ConnectPoints.Select( connectPoint => connectPoint.Diameter / 25.4 * 1000 + "Inch" ).ToList();
        var diameter = component.ConnectPoints.Select( connectPoint => connectPoint.Diameter.NpsInch + "Inch" ).ToList();

        var pipingComponent = new PipingComponent() { Type = componentName, Tag = stockNumber };
        pipingComponent.Diameters.AddRange( diameter );
        pipingComponents.Add( pipingComponent );
      }

      foreach ( var halfVertex in leafEdge.Vertices ) {
        if ( null == halfVertex.Partner ) {
          continue;
        }

        switch ( halfVertex.Flow ) {
          case HalfVertex.FlowType.FromThisToAnother:
            var connectedLeafEdge = halfVertex.Partner.LeafEdge;
            if ( line.LeafEdges.Contains( connectedLeafEdge ) ) {
              LineComponents( connectedLeafEdge, line, pipingComponents, duplicateChecker );
            }
            break;
          case HalfVertex.FlowType.Undefined:
            Debug.LogError( "Error: FlowType is undefined." );
            break;
        }
      }
    }
  }
}
