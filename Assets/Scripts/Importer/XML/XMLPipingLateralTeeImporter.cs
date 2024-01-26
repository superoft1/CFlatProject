using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLPipingLateralTeeImporter : XMLEntityImporter {

  public XMLPipingLateralTeeImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var tee = entity as PipingLateralTee;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d reference = new Vector3d();
    Vector3d mainTerm1 = new Vector3d();
    Vector3d mainTerm2 = new Vector3d();
    Vector3d lateralTerm = new Vector3d();
    double mainDiameter = 0.0;
    double lateralDiameter = 0.0;
    for (int i = 0; i < connectionPoints.Count; ++i)
    {
      var node = connectionPoints[i];
      var pos = GetPosition(node);
      if (i == 0)
      {
        origin = pos;
        axis = GetAxis(node);
        reference = GetReference(node);
      }
      else if (i == 1)
      {
        mainTerm2 = pos;
        mainDiameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        mainTerm1 = pos;
      }
      else if (i == 3)
      {
        lateralTerm = pos;
        lateralDiameter = NominalDiameter(node).OutsideMeter;
      }
    }

    LeafEdgeCodSysUtils.LocalizeLateralTeeComponent(ParentLeafEdge, origin, axis, reference);

    tee.Length1 = (mainTerm1 - origin).magnitude;
    tee.Length2 = (mainTerm2 - origin).magnitude;
    tee.MainDiameter = mainDiameter;
    tee.LateralLength = (lateralTerm - origin).magnitude;
    tee.LateralDiameter = lateralDiameter;
    tee.LateralAxis = ParentLeafEdge.LocalCod.LocalizeVector((lateralTerm - origin).normalized);
    return tee;
  }
}
