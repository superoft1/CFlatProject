using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class CapTypeFrontEndBody : FrontEndBody
	{
    [SerializeField]
    GameObject capBody;

    [SerializeField]
    GameObject cylinderBody;

    public GameObject CapBody { get => capBody; set => capBody = value; }
    public GameObject CylinderBody { get => cylinderBody; set => cylinderBody = value; }
  }
}