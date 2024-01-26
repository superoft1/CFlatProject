using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class RearEndBody : Body
	{
    [SerializeField]
    GameObject flangeBody;

    public GameObject FlangeBody { get => flangeBody; set => flangeBody = value; }
  }
}