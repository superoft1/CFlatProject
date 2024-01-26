using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class CompressorBody : Body
  {
    [SerializeField]
    GameObject foundationBody;

    [SerializeField]
    GameObject equip1Body;

    [SerializeField]
    GameObject driver1Body; // equip1とequip2の間

    [SerializeField]
    GameObject equip2Body;

    [SerializeField]
    GameObject driver2Body; // equip2とequip3の間

    [SerializeField]
    GameObject equip3Body;

    public GameObject FoundationBody { get => foundationBody; set => foundationBody = value; }
    public GameObject Equip1Body { get => equip1Body; set => equip1Body = value; }
    public GameObject ConnectorBody1 { get => driver1Body; set => driver1Body = value; }
    public GameObject Equip2Body { get => equip2Body; set => equip2Body = value; }
    public GameObject ConnectorBody2 { get => driver2Body; set => driver2Body = value; }
    public GameObject Equip3Body { get => equip3Body; set => equip3Body = value; }
  }
}