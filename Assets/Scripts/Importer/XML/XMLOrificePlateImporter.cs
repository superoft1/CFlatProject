using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;

public class XMLOrificePlateImporter : XMLEntityImporter {

  public XMLOrificePlateImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var orifice = entity as OrificePlate;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d term1 = new Vector3d();
    Vector3d term2 = new Vector3d();
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        //orifice.Origin = pos;
      }
      else if (i == 1)
      {
        term1 = pos;
        orifice.Diameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        term2 = pos;
      }
    }

    orifice.Length = ( term1 - term2 ).magnitude ;
    return orifice;
  }
}
