using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class FixHeatExchangerBody : Body
  {
    [SerializeField]
    GameObject tubeBody;

    [SerializeField]
    GameObject flangeBody1;

    [SerializeField]
    GameObject flangeBody2;

    [SerializeField]
    GameObject saddleBody1;

    [SerializeField]
    GameObject saddleBody2;

    [SerializeField]
    GameObject shellCoverBody1;

    [SerializeField]
    GameObject shellCoverBody2;

    public GameObject TubeBody { get => tubeBody; set => tubeBody = value; }
    public GameObject FlangeBody1 { get => flangeBody1; set => flangeBody1 = value; }
    public GameObject FlangeBody2 { get => flangeBody2; set => flangeBody2 = value; }
    public GameObject SaddleBody1 { get => saddleBody1; set => saddleBody1 = value; }
    public GameObject SaddleBody2 { get => saddleBody2; set => saddleBody2 = value; }
    public GameObject ShellCoverBody1 { get => shellCoverBody1; set => shellCoverBody1 = value; }
    public GameObject ShellCoverBody2 { get => shellCoverBody2; set => shellCoverBody2 = value; }
  }
}