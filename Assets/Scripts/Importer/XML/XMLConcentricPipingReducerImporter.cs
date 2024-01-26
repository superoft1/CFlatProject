using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLConcentricPipingReducerImporter : XMLEntityImporter {

  public XMLConcentricPipingReducerImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var reducer = entity as ConcentricPipingReducerCombination;

    Vector3d origin = new Vector3d();
    Vector3d largeTerm = new Vector3d();
    Vector3d smallTerm = new Vector3d();
    double largeDiameter = 0d;
    double smallDiameter = 0d;
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
        largeTerm = pos;
        largeDiameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        smallTerm = pos;
        smallDiameter = NominalDiameter(node).OutsideMeter;
      }
    }
    var direction = largeTerm - smallTerm;

    LeafEdgeCodSysUtils.LocalizeStraightComponent(ParentLeafEdge, origin, direction);

    reducer.LargeDiameter = largeDiameter;
    reducer.SmallDiameter = smallDiameter;
    reducer.Length = (largeTerm - smallTerm).magnitude;

    return reducer;
  }

}
