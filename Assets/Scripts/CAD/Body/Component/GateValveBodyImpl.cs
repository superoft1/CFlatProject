using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class GateValveBodyImpl : MonoBehaviour
  {

    [SerializeField]
    GameObject weld1;

    [SerializeField]
    GameObject weld2;

    public GameObject Weld1
    {
      get
      {
        return weld1;
      }

      set
      {
        weld1 = value;
      }
    }

    public GameObject Weld2
    {
      get
      {
        return weld2;
      }

      set
      {
        weld2 = value;
      }
    }
  }

}