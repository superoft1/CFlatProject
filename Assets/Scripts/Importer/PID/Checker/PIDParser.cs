using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;

namespace PID
{
  class PIDParser
  {
    public static XElement FindPipingNetworkSystem( IEnumerable<XElement> plantModel, string lineID )
    {
      // 表示文字列から探す
      foreach ( var pipingNetworkSystem in plantModel.Elements( "PipingNetworkSystem" ) ) {
        foreach ( var pipingNetworkSegment in pipingNetworkSystem.Elements( "PipingNetworkSegment" ) ) {
          if ( pipingNetworkSegment.XPathSelectElements( $"Label/Text[@String='{lineID}']" ).Any() ) {
            return pipingNetworkSystem ;
          }
        }
      }
      return null;
    }

    public static PipingSystem ParsePipingSystem( XElement pipingNetworkSystem, string lineID )
    {
      var segments = new List<PipingSegment>();
      foreach ( var pipingNetworkSegment in pipingNetworkSystem.Elements( "PipingNetworkSegment" ) ) {
        var segment = ParsePipingSegment( pipingNetworkSegment );
        if ( null != segment ) {
          segments.Add( segment );
        }
      }

      var breaks = new List<PropertyBreak>();
      foreach ( var propertyBreak in pipingNetworkSystem.Elements( "PropertyBreak" ) ) {
        breaks.Add( ParsePropertyBreak( propertyBreak ) );
      }

      if ( segments.Any() || breaks.Any() ) {
        var system = new PipingSystem();
        system.ID = pipingNetworkSystem.Attribute( "ID" ).Value;
        system.LineID = lineID;
        system.Segments.AddRange( segments );
        system.Breaks.AddRange( breaks );
        system.BuildComponentConnections();
        return system;
      }
      return null;
    }

    private static PipingSegment ParsePipingSegment( XElement pipingNetworkSegment )
    {
      List<IConnectable> components = new List<IConnectable>();
      List<PipingSegment.ComponentConnection> connections = new List<PipingSegment.ComponentConnection>();

      foreach ( var element in pipingNetworkSegment.Elements() ) {
        switch ( element.Name.LocalName ) {
          case "PipingComponent":
          case "ProcessInstrument":
            var fitting = ParsePipingFitting( element );
            if ( null != fitting ) {
              components.Add( fitting );
            }
            break;
          case "PipeConnectorSymbol":
            var connector = ParseConnectorSymbol( element );
            if ( null != connector ) {
              components.Add( connector );
            }
            break;
          case "Connection":
            var connection = ParseConnection( element );
            if ( null != connection ) {
              connections.Add( connection );
            }
            break;
        }
      }

      if ( components.Any() ) {
        var segment = new PipingSegment();
        segment.ID = pipingNetworkSegment.Attribute( "ID" ).Value;
        segment.Specification = ParseSpecification( pipingNetworkSegment );
        segment.Diameter = ParseNominalDiameter( pipingNetworkSegment );
        segment.Components.AddRange( components );
        segment.Connection = connections.Single(); // 唯一のはず
        components.ForEach( component => component.Owner = segment );
        return segment;
      }
      return null ;
    }

    private static PipingFitting ParsePipingFitting( XElement element )
    {
      var type = element.Attribute( "ComponentClass" ).Value;
      if ( "Flange" == type || "BlindFlange" == type ) {
        return null; // Flangeは出力対象外
      }

      var diameters = ParseConnectionPointDiameters( element );
      if ( null == diameters ) {
        return null;
      }

      var fitting = new PipingFitting();
      fitting.ID = element.Attribute( "ID" ).Value;
      fitting.Type = type;
      fitting.Tag = element.Attribute( "TagName" )?.Value ?? "-"; // 存在しない場合あり
      fitting.Diameters.AddRange( diameters );
      return fitting;
    }

    private static PipeConnectorSymbol ParseConnectorSymbol( XElement element )
    {
      var connector = new PipeConnectorSymbol();
      connector.ID = element.Attribute( "ID" ).Value;
      connector.Type = element.Attribute( "ComponentClass" ).Value;
      var linkedPersistentID = element.XPathSelectElements( "CrossPageConnection/LinkedPersistentID" ).Single();
      connector.LinkedPersistentID = linkedPersistentID.Attribute( "Identifier" ).Value;
      return connector;
    }

    private static PipingSegment.ComponentConnection ParseConnection( XElement element )
    {
      var connection = new PipingSegment.ComponentConnection();
      connection.From = ( element.Attribute( "FromID" ).Value,
                          int.Parse( element.Attribute( "FromNode" )?.Value ?? "0" ) - 1 ); // ConnectionPointsのDiameterが有効なのは1番目以降のNode
      connection.To = ( element.Attribute( "ToID" ).Value,
                        int.Parse( element.Attribute( "ToNode" )?.Value ?? "0" ) - 1 );
      return connection;
    }

    private static string ParseSpecification( XElement pipingNetworkSegment )
    {
      var genericAttribute
        = pipingNetworkSegment.XPathSelectElements( "GenericAttributes/GenericAttribute[@Name='PipingMaterialsClass']" ).Single() ;
      return genericAttribute.Attribute( "Value" ).Value;
    }

    private static string ParseNominalDiameter( XElement pipingNetworkSegment )
    {
//      var genericAttribute
//          = pipingNetworkSegment.XPathSelectElements( "GenericAttributes/GenericAttribute[@Name='NominalDiameter']" ).Single() ;
//      return genericAttribute.Attribute( "Value" ).Value;

      // ConnectionPoints内のNominalDiameterと同一表記を得るために以下の情報を利用する
      var nominalDiameters = pipingNetworkSegment.Element( "NominalDiameter" );
      return nominalDiameters.Attribute( "Value" ).Value + nominalDiameters.Attribute( "Units" ).Value;
    }

    private static List<string> ParseConnectionPointDiameters( XElement element )
    {
      var nominalDiameters = element.XPathSelectElements( "ConnectionPoints/Node/NominalDiameter" );
      if ( !nominalDiameters.Any() ) {
        return null;
      }

      // TODO: ConnectionPoints内で単位系が異なるパターンには未対応
      var units = nominalDiameters.First().Attribute( "Units" ).Value;
      if ( nominalDiameters.Any( nominalDiameter => nominalDiameter.Attribute( "Units" ).Value != units ) ) {
        Debug.LogError( "Error: Diameter unit is not same." );
        return null;
      }

      return nominalDiameters.Select( nominalDiameter => nominalDiameter.Attribute( "Value" ).Value + units ).ToList();
    }

    private static PropertyBreak ParsePropertyBreak( XElement propertyBreak )
    {
      var break_ = new PropertyBreak();
      break_.ID = propertyBreak.Attribute( "ID" ).Value;
      break_.Type = propertyBreak.Attribute( "ComponentClass" ).Value;
      return break_;
    }
  }
}