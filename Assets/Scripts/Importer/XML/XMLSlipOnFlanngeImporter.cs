﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLSlipOnFlannge : XMLEntityImporter {

  public XMLSlipOnFlannge(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var slipOnFlange = entity as SlipOnFlange;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d outsideTerm = new Vector3d();
    Vector3d weldTerm = new Vector3d();
    double diameter = 0.0;
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
        outsideTerm = pos;
        diameter = NominalDiameter(node).OutsideMeter * 2.0;
      }
      else if (i == 2)
      {
        weldTerm = pos;
      }
    }
    var direction = weldTerm - outsideTerm;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    slipOnFlange.Diameter = diameter;
    slipOnFlange.Length = (weldTerm - outsideTerm).magnitude;

    return slipOnFlange;
  }
}