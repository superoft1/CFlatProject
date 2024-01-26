using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLGateValveImporter : XMLEntityImporter {

  public XMLGateValveImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var valve = entity as GateValve;

    var origin = new Vector3d();
    var term1 = new Vector3d();
    var term2 = new Vector3d();
    double diameter = 0d;
    var connectionPoints = GetConnectionPoints(element);
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
        term1 = pos;
        diameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        term2 = pos;
      }
    }

    var direction = (term2 - term1).normalized;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    valve.Diameter = diameter;
    valve.Length = (term2 - term1).magnitude;

    return valve;
  }
	
}
