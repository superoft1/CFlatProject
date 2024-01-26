using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Importer.Checker;
using UnityEngine;

namespace PID
{
  class PIDAnalysis
  {
    private Dictionary<string, PipingModel> _models = null;

    public PIDAnalysis( string[] files, string[] lineIDs )
    {
      BuildPipingModels( files, lineIDs );
    }

    public void LineComponents( List<PipingLine> pipingLines )
    {
      foreach ( var model in _models ) {
        var specifications = new List<string>();
        var lineComponents = new List<PipingComponent>();
        var duplicateChecker = new HashSet<string>();
        foreach ( var component in CollectRootConnectableComponents( model.Value ) ) {
          var specification = LineComponents( component, lineComponents, duplicateChecker );
          specifications.Add( specification );
        }

        if ( lineComponents.Any() ) {
          var pipingLine = new PipingLine() { LineID = model.Key, Specification = specifications.Distinct().Single() };
          pipingLine.Components.AddRange( lineComponents );
          pipingLines.Add( pipingLine );
        }
      }
    }

    public void OutputComponentTables()
    {
      var path = Application.dataPath + "/Outputs/PIDComponentTable.csv";
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

    private void BuildPipingModels( string[] files, string[] lineIDs )
    {
      _models = new Dictionary<string, PipingModel>();

      foreach ( var file in files ) {
        var documentRoot = XDocument.Load( file );
        var plantModel = documentRoot.Elements( "PlantModel" );

        foreach ( var lineID in lineIDs ) {
          var pipingNetworkSystem = PIDParser.FindPipingNetworkSystem( plantModel, lineID );
          if ( null == pipingNetworkSystem ) {
            continue;
          }

          var system = PIDParser.ParsePipingSystem( pipingNetworkSystem, lineID );
          if ( null != system ) {
            if ( !_models.TryGetValue( lineID, out PipingModel model ) ) {
              model = new PipingModel();
              _models.Add( lineID, model );
            }
            model.Systems.Add( system );
          }
        }
      }

      foreach ( var model in _models ) {
        model.Value.BuildCrossPageConnections();
      }
    }

    private IEnumerable<IConnectable> CollectRootConnectableComponents( PipingModel model )
    {
      var rootComponents = new HashSet<IConnectable>();

      var segments = model.Systems.SelectMany( system => system.Segments );
      foreach ( var segment in segments ) {
        foreach ( var rootComponent in FindRootConnectableComponent( segment.Components.First() ) ) {
          rootComponents.Add( rootComponent );
        }
      }

      return rootComponents;
    }

    private IEnumerable<IConnectable> FindRootConnectableComponent( IConnectable component )
    {
      if ( !component.Parents.Any() ) {
        yield return component;
      }

      foreach ( var parent in component.Parents ) {
        var segment = parent.component.Owner;
        foreach ( var rootComponent in FindRootConnectableComponent( null != segment ? segment.Components.First() : parent.component ) ) {
          yield return rootComponent;
        }
      }
    }

    private string LineComponents( IConnectable component, List<PipingComponent> lineComponents, HashSet<string> duplicateChecker )
    {
      string specification = null;

      List<( IConnectable component, int index )> children = null;

      var segment = component.Owner;
      if ( null != segment ) {
        specification = segment.Specification;

        foreach ( var fitting in segment.Components.SkipWhile( connectable => connectable != component ) // 途中から始まる場合あり
                                                   .OfType<IPipingFitting>() ) {
          if ( duplicateChecker.Add( fitting.ID ) ) {
            if ( fitting.Type == "PipingNetworkBranch" && fitting.Diameters.Count != 3 ) {
              continue; // 謎なので無視する...
            }

            var lineComponent = new PipingComponent() { Type = fitting.Type, Tag = fitting.Tag };
            lineComponent.Diameters.AddRange( fitting.Diameters );
            lineComponents.Add( lineComponent );
          }
        }

        children = segment.Components.Last().Children;
      }
      else {
        children = component.Children; // PropertyBreakの場合
      }

      foreach ( var child in children ) {
        var childSpecification = LineComponents( child.component, lineComponents, duplicateChecker );

        if ( null == specification ) {
          specification = childSpecification;
        }
        else if ( null != childSpecification && specification != childSpecification ) {
          Debug.LogError( "Error: Specification is not same." );
          lineComponents.Clear();
          return null;
        }
      }

      return specification;
    }
  }
}
