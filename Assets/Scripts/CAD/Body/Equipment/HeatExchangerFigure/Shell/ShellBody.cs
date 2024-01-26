using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class ShellBody : Body
	{
    [SerializeField]
    GameObject frontEndFlangeBody;

    [SerializeField]
    GameObject rearEndFlangeBody;

    [SerializeField]
    GameObject saddle1Body;

    [SerializeField]
    GameObject saddle2Body;

    public GameObject FrontEndFlangeBody { get => frontEndFlangeBody; set => frontEndFlangeBody = value; }
    public GameObject RearEndFlangeBody { get => rearEndFlangeBody; set => rearEndFlangeBody = value; }
    public GameObject Saddle1Body { get => saddle1Body; set => saddle1Body = value; }
    public GameObject Saddle2Body { get => saddle2Body; set => saddle2Body = value; }
  }
}