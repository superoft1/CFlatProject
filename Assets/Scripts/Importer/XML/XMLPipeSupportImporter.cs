using System.Collections.Generic;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

public class XMLPipeSupportImporter : XMLEntityImporter
{

  public XMLPipeSupportImporter( EntityType.Type _type, System.Xml.XmlElement _element ) : base( _type, _element )
  {
  }

  public override Entity Import( Chiyoda.CAD.Core.Document doc )
  {
    var entity = doc.CreateEntity( type );
    var support = entity as Support;

    var position = GetPosition( element );
    var block = Group as BlockPattern;
    if ( block != null ) {
      position = block.LocalCod.LocalizePoint( position );
    }
    var assocPipe = EntityDictionary.GetEntityFromID( AssociationID( element ) );

    SupportPositionBase pos;
    if ( assocPipe is Pipe ) {
      pos = new PipeSupportPosition( support );
      var assocLeafEdge = assocPipe.Parent as LeafEdge;
      pos.Target = assocLeafEdge;
    }
    else if ( null != assocPipe && null != assocPipe.Parent ) {
      pos = new RelativeSupportPosition( support );
      var assocLeafEdge = assocPipe.Parent as LeafEdge;
      pos.Target = assocLeafEdge;
    }
    else {
      pos = new AbsoluteSupportPosition( support );
    }
    pos.Position = position;
    support.SupportPosition = pos;
    support.SupportType = SupportType.None;

    var elmWithSupport = (Group as ISupportParentElement) ?? Document;
    elmWithSupport.Supports.Add( support );

    return support;
  }

  private static string AssociationID( System.Xml.XmlNode node )
  {
    var value = node["Association"].GetAttribute( "ItemID" );
    return value;
  }
}
