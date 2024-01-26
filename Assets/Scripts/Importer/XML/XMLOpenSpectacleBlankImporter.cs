using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLOpenSpectacleBlankImporter : XMLEntityImporter {

  public XMLOpenSpectacleBlankImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var blank = entity as OpenSpectacleBlank;

    Vector3d origin = new Vector3d();
    Vector3d term1 = new Vector3d();
    Vector3d term2 = new Vector3d();
    double diameter = 0;
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
    var direction = term2 - term1;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    blank.Diameter = diameter;
    blank.Length = (term2 - term1).magnitude;
    return blank;
  }
}
