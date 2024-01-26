using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLImporterUtility {

  public static bool IsPipingLateralTee(System.Xml.XmlElement element)
  {
    var points = XMLEntityImporter.GetConnectionPoints(element);
    var target = points[0];
    var axis = XMLEntityImporter.GetAxis(target);
    var reference = XMLEntityImporter.GetReference(target);
    return System.Math.Abs(Vector3d.Dot(axis, reference)) > 0.01;
  }

}
