using System;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class ChillerBody : Body
  {
    [SerializeField]
    GameObject tubeBody;

    [SerializeField]
    GameObject capBody1;

    [SerializeField]
    GameObject capBody2;

    [SerializeField]
    GameObject tipCylinderBody1;

    [SerializeField]
    GameObject tipCylinderBody2;

    [SerializeField]
    GameObject tipTaperBody1;

    [SerializeField]
    GameObject tipTaperBody2;

    [SerializeField]
    GameObject saddleBody1;

    [SerializeField]
    GameObject saddleBody2;

    public GameObject TubeBody { get => tubeBody; set => tubeBody = value; }
    public GameObject CapBody1 { get => capBody1; set => capBody1 = value; }
    public GameObject CapBody2 { get => capBody2; set => capBody2 = value; }
    public GameObject TipCylinderBody1 { get => tipCylinderBody1; set => tipCylinderBody1 = value; }
    public GameObject TipCylinderBody2 { get => tipCylinderBody2; set => tipCylinderBody2 = value; }
    public GameObject TipTaperBody1 { get => tipTaperBody1; set => tipTaperBody1 = value; }
    public GameObject TipTaperBody2 { get => tipTaperBody2; set => tipTaperBody2 = value; }
    public GameObject SaddleBody1 { get => saddleBody1; set => saddleBody1 = value; }
    public GameObject SaddleBody2 { get => saddleBody2; set => saddleBody2 = value; }
  }
}