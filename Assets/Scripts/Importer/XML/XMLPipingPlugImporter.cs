using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLPipingPlugImporter : XMLEntityImporter {

  public XMLPipingPlugImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var pipingPlug = entity as PipingPlug;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d weldTerm = new Vector3d();
    Vector3d outsideTerm = new Vector3d();
    double diameter = 0.0;
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        origin = pos;
      }
      else if (i == 1)
      {
        weldTerm = pos;
        diameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        outsideTerm = pos;
      }
    }
    var direction = outsideTerm - weldTerm;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    pipingPlug.Diameter = diameter;
    pipingPlug.Length = (outsideTerm - weldTerm).magnitude;

    return pipingPlug;
  }
}
