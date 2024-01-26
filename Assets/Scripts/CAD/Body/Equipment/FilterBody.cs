using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class FilterBody : Body
  {
    [SerializeField]
    GameObject cylinderBody;

    [SerializeField]
    GameObject northLegBody;

    [SerializeField]
    GameObject southLegBody;

    [SerializeField]
    GameObject eastLegBody;

    [SerializeField]
    GameObject westLegBody;

    [SerializeField]
    GameObject flangeBody;

    [SerializeField]
    GameObject capBody;

    public GameObject CylinderBody { get => cylinderBody; set => cylinderBody = value; }
    public GameObject NorthLegBody { get => northLegBody; set => northLegBody = value; }
    public GameObject SouthLegBody { get => southLegBody; set => southLegBody = value; }
    public GameObject EastLegBody { get => eastLegBody; set => eastLegBody = value; }
    public GameObject WestLegBody { get => westLegBody; set => westLegBody = value; }
    public GameObject FlangeBody { get => flangeBody; set => flangeBody = value; }
    public GameObject CapBody { get => capBody; set => capBody = value; }
  }
}
