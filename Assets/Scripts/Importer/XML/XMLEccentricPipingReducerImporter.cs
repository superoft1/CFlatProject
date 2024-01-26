using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;

public class XMLEccentricPipingReducerImporter : XMLEntityImporter
{

  public XMLEccentricPipingReducerImporter(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }
	
  public override Entity Import(Document doc)
  {
    var entity = doc.CreateEntity(type);
    var reducer = entity as EccentricPipingReducerCombination;

    var position = GetPosition(element);
    var axis = GetAxis(element);
    var reference = GetReference(element);
    var outer = Vector3d.Cross(axis, reference);

    reference = axis.normalized;
    axis = Vector3d.Cross(outer, axis).normalized;

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

    //! Axisが逆になっている場合があるので、座標からチェック
    if (Vector3d.Dot(axis, (smallTerm - largeTerm)) > 0)
    {
      axis *= -1;
    }


    LeafEdgeCodSysUtils.LocalizeEccentricPipingReducerComponent(ParentLeafEdge, origin, axis, reference);

    reducer.LargeDiameter = largeDiameter;
    reducer.SmallDiameter = smallDiameter;
    reducer.Length = (largeTerm - smallTerm).magnitude;


    return reducer;
  }
}
