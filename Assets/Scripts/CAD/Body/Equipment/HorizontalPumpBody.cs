using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class HorizontalPumpBody : Body
  {
    [SerializeField]
    GameObject basePlateBody;

    [SerializeField]
    GameObject impellerBody;

    [SerializeField]
    GameObject motorBody;

    [SerializeField]
    GameObject driverBody;

    public GameObject BasePlateBody { get => basePlateBody; set => basePlateBody = value; }
    public GameObject ImpellerBody { get => impellerBody; set => impellerBody = value; }
    public GameObject MotorBody { get => motorBody; set => motorBody = value; }
    public GameObject DriverBody { get => driverBody; set => driverBody = value; }
  }
}
