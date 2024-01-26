﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLSockOletImporter : XMLEntityImporter {
  
  public XMLSockOletImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var olet = entity as SockOlet;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d mainTerm = new Vector3d();
    Vector3d branchTerm = new Vector3d();
    double diameter = 0.0;
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        mainTerm = pos;
      }
      else if (i == 3)
      {
        branchTerm = pos;
        diameter = NominalDiameter(node).OutsideMeter;
      }
    }
    var direction = branchTerm - mainTerm;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, 0.5 * (mainTerm + branchTerm), direction);

    olet.Diameter = diameter;
    olet.Length = (branchTerm - mainTerm).magnitude;

    return olet;
  }

}
