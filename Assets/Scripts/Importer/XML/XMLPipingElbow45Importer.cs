using System ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

public class XMLPipingElbow45Importer : XMLEntityImporter {

  public XMLPipingElbow45Importer(EntityType.Type _type, System.Xml.XmlElement _element) : base(_type, _element)
  {
  }

  public override Entity Import(Chiyoda.CAD.Core.Document doc)
  {
    var entity = doc.CreateEntity(type);
    var elbow = entity as PipingElbow45;

    var connectionPoints = GetConnectionPoints(element);
    Vector3d origin = new Vector3d();
    Vector3d term1 = new Vector3d();
    Vector3d term2 = new Vector3d();
    Vector3d axis = new Vector3d();
    Vector3d reference = new Vector3d();
    Diameter diameter = Diameter.Default();
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
        term1 = pos;
        diameter = NominalDiameter(node);
      }
      else if (i == 2)
      {
        term2 = pos;
      }
    }

    LeafEdgeCodSysUtils.LocalizeElbow45Component(ParentLeafEdge, origin, axis, reference);

    elbow.ChangeSizeNpsMm( 0, diameter.NpsMm ) ;
    elbow.BendLength = ParentLeafEdge.GlobalCod.LocalizePoint(term1).magnitude;

    return elbow;
  }
}
