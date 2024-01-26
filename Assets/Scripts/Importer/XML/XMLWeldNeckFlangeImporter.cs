using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLWeldNeckFlangeImporter : XMLEntityImporter {

  public XMLWeldNeckFlangeImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var flange = entity as WeldNeckFlange;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d weld = new Vector3d();
    Vector3d outside = new Vector3d();
    double diameter = 0;
    double outsideDiameter = 0;
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        origin = pos;
        axis = GetAxis(node);        
      }
      else if (i == 1)
      {
        weld = pos;
        diameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        outside = pos;
        outsideDiameter = NominalDiameter(node).OutsideMeter;
      }
    }
    var direction = weld - outside;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    flange.WeldDiameter = diameter;
    flange.OutsideDiameter = diameter;
    flange.Length = (outside - weld).magnitude;

    return flange;
  }

}
