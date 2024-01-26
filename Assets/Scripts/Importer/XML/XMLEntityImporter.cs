using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Chiyoda.Importer;

public abstract class XMLEntityImporter
{

  protected EntityType.Type type;
  protected System.Xml.XmlElement element;

  static double coordScale = 0.001d;

  public XMLEntityImporter(EntityType.Type _type, System.Xml.XmlElement _element)
  {
    type = _type;
    element = _element;
  }

  public LeafEdge ParentLeafEdge { get; set; }
  public IGroup Group { get; internal set; }
  public Document Document { get; internal set; }
  public IEntityDictionary EntityDictionary { get; internal set; }

  public abstract Entity Import(Document doc);

  public static Diameter NominalDiameter( System.Xml.XmlNode node )
  {
    var value = double.Parse(node["NominalDiameter"].GetAttribute("Value"));
    return node[ "NominalDiameter" ].GetAttribute( "Units" ) == "in" ?
      DiameterFactory.FromNpsInch( value ) :
      DiameterFactory.FromNpsMm( value ) ;
  }

  public static List<System.Xml.XmlNode> GetConnectionPoints(System.Xml.XmlElement element)
  {
    var connectionPoints = element["ConnectionPoints"];
    List<System.Xml.XmlNode> nodes = new List<System.Xml.XmlNode>();
    for (int i = 0; i < connectionPoints.ChildNodes.Count; ++i) {
      nodes.Add(connectionPoints.ChildNodes[i]);
    }
    return nodes;
  }

  public static bool HasAxis(System.Xml.XmlNode node)
  {
    var position = node["Position"];
    if (position == null) return false;
    var axis = position["Axis"];
    if (axis == null) return false;
    return true;
  }

  public static Vector3d GetAxis(System.Xml.XmlNode node)
  {
    var location = node["Position"]["Axis"];
    var x = -double.Parse(location.GetAttribute("X"));
    var y = double.Parse(location.GetAttribute("Y"));
    var z = double.Parse(location.GetAttribute("Z"));
    return new Vector3d(x, y, z);
  }

  public static bool HasReference(System.Xml.XmlNode node)
  {
    var position = node["Position"];
    if (position == null) return false;
    var axis = position["Reference"];
    if (axis == null) return false;
    return true;
  }

  public static Vector3d GetReference(System.Xml.XmlNode node)
  {
    var location = node["Position"]["Reference"];
    var x = -double.Parse(location.GetAttribute("X"));
    var y = double.Parse(location.GetAttribute("Y"));
    var z = double.Parse(location.GetAttribute("Z"));
    return new Vector3d(x, y, z);
  }

  public static Vector3d GetPosition(System.Xml.XmlNode node)
  {
    var location = node["Position"]["Location"];
    var x = -double.Parse(location.GetAttribute("X")) * coordScale;
    var y = double.Parse(location.GetAttribute("Y")) * coordScale;
    var z = double.Parse(location.GetAttribute("Z")) * coordScale;
    return new Vector3d(x, y, z);
  }

}
