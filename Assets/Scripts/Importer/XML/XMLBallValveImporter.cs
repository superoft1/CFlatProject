using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLBallValveImporter : XMLEntityImporter {

  public XMLBallValveImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d weld1Term = new Vector3d();
    Vector3d weld2Term = new Vector3d();
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
        weld1Term = pos;
        diameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        weld2Term = pos;
      }
    }
    var direction = weld2Term - weld1Term;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    var valve = entity as BallValve;
    valve.Diameter = diameter;
    valve.Length = (weld2Term - weld1Term).magnitude;

    return valve;
  }
}
