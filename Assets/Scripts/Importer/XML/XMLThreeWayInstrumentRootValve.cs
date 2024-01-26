using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLThreeWayInstrumentRootValve : XMLEntityImporter {

  public XMLThreeWayInstrumentRootValve(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }
	
  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var threeWayInstrumentRootvalve = entity as ThreeWayInstrumentRootvalve;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d reference = new Vector3d();
    Vector3d axisRightTerm = new Vector3d();
    Vector3d axisLeftTerm = new Vector3d();
    double axisDiameter = 0.0;
    Vector3d referenceTerm = new Vector3d();
    double referenceDiameter = 0.0;
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
        axisRightTerm = pos;
        axisDiameter = NominalDiameter(node).OutsideMeter;
      }
      else if (i == 2)
      {
        axisLeftTerm = pos;
      }
      else if (i == 3)
      {
        referenceTerm = pos;
        referenceDiameter = NominalDiameter(node).OutsideMeter;
      }
    }

    LeafEdgeCodSysUtils.LocalizeThreeWayInstrumentRootValveComponent(ParentLeafEdge, origin, axis, reference);

    threeWayInstrumentRootvalve.AxisDiameter = axisDiameter;
    threeWayInstrumentRootvalve.ReferenceDiameter = referenceDiameter;
    threeWayInstrumentRootvalve.AxisLength1 = (axisLeftTerm - origin).magnitude;
    threeWayInstrumentRootvalve.AxisLength2 = (axisRightTerm - origin).magnitude; 
    threeWayInstrumentRootvalve.ReferenceLength = (referenceTerm - origin).magnitude;

    return threeWayInstrumentRootvalve;
  }
}
