using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class WeldNeckFlangeBodyImpl : MonoBehaviour
  {

    [SerializeField]
    GameObject weld;

    [SerializeField]
    GameObject outside;

    public GameObject Weld
    {
      get
      {
        return weld;
      }

      set
      {
        weld = value;
      }
    }

    public GameObject Outside
    {
      get
      {
        return outside;
      }

      set
      {
        outside = value;
      }
    }
  }

}