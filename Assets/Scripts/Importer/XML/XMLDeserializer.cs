using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.CAD.Core;
using System;
using Chiyoda.Importer;

public class XMLDeserializer : MonoBehaviour, IEntityDictionary
{
  private readonly Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();

  public BlockPattern ImportData( BlockPattern bp, Document doc, string[] pathList)
  {
    foreach ( var path in pathList ) {
      var grpInfo = new GroupInfo( doc, bp, path );
      ImportData( path, grpInfo );
    }
    return bp;
  }

  public void ImportData( Document doc, string path )
  {
    ImportData( path, new GroupInfo( doc, doc, path ) );
  }

  private void ImportData( string path, GroupInfo grpInfo )
  {
    this._entities.Clear();

    string content = System.IO.File.ReadAllText( path );
    var xml = new System.Xml.XmlDocument();
    xml.LoadXml( content );

    ImportData( xml.DocumentElement, grpInfo );
  }

  string LineId( string id )
  {
    //FIXME: 連番が振られる規則がわからないので、しばらくはそのまま返す
    return id;
  }

  private void ImportData( System.Xml.XmlElement i_element, GroupInfo grpInfo )
  {
    var element = i_element["PipingNetworkSystem"];
    var lineId = LineId( element.GetAttribute( "Tag" ) );
    var serviceClass = element.GetAttribute( "Specification" );

    var (line, group) = grpInfo.GetLineAndGroup( lineId );
    line.ServiceClass = serviceClass;

    using ( Group.ContinuityIgnorer( group ) ) {
      foreach ( var child in element.ChildNodes ) {
        var childElement = child as System.Xml.XmlElement;
        if ( childElement != null ) {
          if ( childElement.Name == "PipingNetworkSegment" ) {
            ImportSegment( grpInfo.Document, childElement, group, line );
          }
        }
      }
    }

    foreach ( var child in i_element.ChildNodes)
    {
      var childElement = child as System.Xml.XmlElement;
      if (childElement != null && childElement.Name == "PipeSupport")
      {
        ImportPipeSupport( grpInfo.Document, childElement, grpInfo.IGroup );
      }
    }
  }

  void ImportPipeSupport( Document doc, System.Xml.XmlElement i_element, IGroup group )
  {
    var stockNumber = i_element.GetAttribute( "StockNumber" );
    var entity = XMLEntityFactory.ImportEntity( doc, this, EntityType.Type.Support, i_element, group, null );
    if ( entity == null ) return;

    // TODO: ストックナンバーの対応を後で考える
    //var pipingPiece = entity as PipingPiece;
    //var component = pipingPiece as Chiyoda.CAD.Model.Component;
    //if (component == null) return;
    //component.StockNumber = stockNumber;
  }

  private void ImportSegment( Document doc, System.Xml.XmlElement i_element, IGroup group, Line line )
  {
    foreach ( var child in i_element.ChildNodes ) {
      var childElement = child as System.Xml.XmlElement;
      if ( childElement != null ) {
        if ( childElement.Name == "PipingComponent" || childElement.Name == "Pipe" || childElement.Name == "ProcessInstrument" ) {
          var componentName = childElement.GetAttribute( "ComponentClass" );
          var stockNumber = childElement.GetAttribute( "StockNumber" );
          var entityType = XMLEntityType.GetType( componentName );
          if ( entityType == EntityType.Type.PipingTee ) {
            //! TeeとLateralTeeの判別
            if ( XMLImporterUtility.IsPipingLateralTee( childElement ) ) {
              entityType = EntityType.Type.PipingLateralTee;
            }
          }
          else if ( entityType == EntityType.Type.UNKNOWN_COMPONENT ) {
            entityType = EntityType.Type.BlankSpectacleBlank;
          }

          var leafEdge = doc.CreateEntity<LeafEdge>();
          group.AddEdge( leafEdge );
          var entity = XMLEntityFactory.ImportEntity( doc, this, entityType, childElement, group, leafEdge );
          if ( entity == null ) continue;

          if ( childElement.HasAttribute( "ID" ) ) {
            RegisterEntity( childElement.GetAttribute( "ID" ), entity );
          }

          leafEdge.Line = line;

          var pipingPiece = entity as PipingPiece;
          leafEdge.PipingPiece = pipingPiece;
          doc.CreateHalfVerticesAndMakePairs( leafEdge );

          var component = pipingPiece as Chiyoda.CAD.Model.Component;
          if ( component == null ) continue;
          component.StockNumber = stockNumber;
        }
      }
    }
  }

  private void RegisterEntity( string id, Entity entity )
  {
    if ( _entities.ContainsKey( id ) ) return;

    _entities.Add( id, entity );
  }

  public Entity GetEntityFromID( string id )
  {
    if ( _entities.TryGetValue( id, out var entity ) ) return entity;

    return null;
  }
}
