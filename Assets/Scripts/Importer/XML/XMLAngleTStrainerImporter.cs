using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;

public class XMLAngleTStrainerImporter : XMLEntityImporter {

  public XMLAngleTStrainerImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var tee = entity as AngleTStrainer;

    var connectionPoints = GetConnectionPoints(element);
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        //tee.Origin = pos;
        //tee.Reference = GetAxis(node);
        //tee.Axis = GetReference(node);
      }
      else if (i == 1)
      {
        tee.ConvexTerm = pos;
        tee.ConvexDiameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        tee.AxisTerm = pos;
        tee.AxisDiameter = NominalDiameter(node).OutsideMeter;
      }
    }
    return tee;
  }
}
