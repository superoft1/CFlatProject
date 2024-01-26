using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLPipingTeeImporter : XMLEntityImporter {

  public XMLPipingTeeImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }
	
  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var tee = entity as PipingTee;

    Vector3d origin = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d reference = new Vector3d();
    Vector3d mainTerm1 = new Vector3d();
    Vector3d mainTerm2 = new Vector3d();
    double mainDiameter = 0d;
    Vector3d branchTerm = new Vector3d();
    double branchDiameter = 0d;

    var connectionPoints = GetConnectionPoints(element);
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
        branchTerm = pos;
        branchDiameter = NominalDiameter(node).OutsideMeter;
      }
    }

    LeafEdgeCodSysUtils.LocalizeTeeComponent(ParentLeafEdge, origin, axis, reference);

    tee.MainDiameter = mainDiameter;
    tee.BranchDiameter = branchDiameter;
    tee.MainLength = (mainTerm2 - mainTerm1).magnitude;
    tee.BranchLength = (branchTerm - origin).magnitude;
    //elbow.Term1 = ParentLeafEdge.LeafEdgeCodSys.LocalizePoint(term1);
    //elbow.Term2 = ParentLeafEdge.LeafEdgeCodSys.LocalizePoint(term2);

    return tee;
  }
}
